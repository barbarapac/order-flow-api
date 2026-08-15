# ADR-011 — JWT de 60 minutos, sem refresh token

## Contexto

Todo endpoint autenticado depende de um token emitido no login. É preciso definir por quanto tempo ele vale e se
existe renovação sem novo login.

## Opções

- **Token curto mais refresh token.** Reduz a janela de uso de um token vazado, mas exige emissão,
  armazenamento e revogação do refresh — fora do escopo pedido.
- **Token mais longo, sem refresh.** Implementação simples, sem infraestrutura extra, com janela de exposição
  maior caso o token vaze.

## Decisão

O token expira em 60 minutos. Não há refresh token nesta fase.

## Consequências

- Sessenta minutos são suficientes para uso manual pelo Swagger durante a avaliação, sem forçar login repetido a
  cada teste. Não é um valor pensado para produção, onde a política equilibraria janela de exposição contra
  fricção de login e viria acompanhada de refresh token.
- Quando o token expira, a única saída é logar de novo. Aceitável no escopo do desafio, ponto explícito a
  revisitar se o projeto crescer.

## Referências

- [RFC 7519 — JSON Web Token](https://www.rfc-editor.org/rfc/rfc7519)
