# ADR-015 — CORS com origens explícitas vindas de configuração

## Contexto

Um frontend próprio consome a API de outra origem (`http://localhost:5173` no dev server), então o navegador
exige CORS. A API é stateless e autentica por JWT no header `Authorization` — não há cookie de sessão, logo não
há necessidade de credenciais cross-origin.

## Opções

- **`AllowAnyOrigin`**. Resolve o desenvolvimento em uma linha e nunca mais dá trabalho. É o que a documentação
  da Microsoft desaconselha explicitamente: combinado com `AllowCredentials` a própria lib recusa a política,
  por risco de CSRF, e mesmo sem credenciais expõe a API a qualquer página do navegador do usuário.
- **Origens explícitas fixas no código.** Seguro, mas obriga recompilar para publicar o frontend em outro
  domínio.
- **Origens explícitas vindas de configuração**, com uma política nomeada aplicada globalmente.

## Decisão

Política nomeada `Frontend` (`CorsPolicies.Frontend`), com as origens lidas de `Cors:AllowedOrigins` e
`AllowAnyHeader`/`AllowAnyMethod`, sem `AllowCredentials`. `UseCors` fica depois do roteamento e antes de
`UseAuthentication`/`UseAuthorization`, como a documentação exige, para que a resposta carregue os headers de
CORS tanto em chamadas autorizadas quanto em `401`.

Quando a configuração está ausente, o comportamento depende do ambiente: em `Development` caem as origens
padrão do dev server (`localhost` e `127.0.0.1` — o navegador trata as duas como origens distintas); fora dele,
o startup falha com `InvalidOperationException`, no mesmo estilo das connection strings em
`Infrastructure/IoC.cs`. Um fallback silencioso para `localhost` em produção esconderia um erro de
configuração em vez de denunciá-lo.

As origens são normalizadas com `TrimEnd('/')`: uma barra final faz a comparação falhar sem nenhum erro no
servidor, e o sintoma aparece só como bloqueio no navegador — armadilha fácil de cair quando o valor vem de
variável de ambiente.

## Consequências

- Publicar o frontend em outro domínio é mudar `Cors__AllowedOrigins__0`, sem recompilar.
- Esquecer a variável em produção derruba o boot com mensagem clara, em vez de subir liberando `localhost`.
- `SetPreflightMaxAge` de 10 minutos reduz o `OPTIONS` repetido a cada chamada.
- Se algum dia a autenticação passar a usar cookie, `AllowCredentials` será necessário — e aí as origens
  explícitas já em vigor passam a ser obrigatórias, não só recomendáveis.

## Referências

- [ASP.NET Core — Enable CORS](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [ASP.NET Core — Middleware order](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/#middleware-order)
