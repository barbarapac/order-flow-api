# ADR-013 — Handler global de exception e tabela única de status

## Contexto

O enunciado pede fastfail e respostas de erro em `ProblemDetails`, geradas em um único ponto, não espalhadas em
`try/catch` por endpoint. Só que existem três origens de erro bem diferentes no projeto — validação de payload,
exception de domínio e "não encontrado" — e é preciso decidir como todas convergem para o mesmo formato sem
duplicar a lógica de mapeamento.

## Opções

- **Middleware customizado de exception**, padrão comum em projetos .NET anteriores ao .NET 8. Funciona, mas
  reinventa um encanamento que o framework passou a oferecer.
- **`IExceptionHandler`**, o mecanismo nativo. Mesmo objetivo, registrado pelo próprio framework.
- Para o caminho sem exception, duas saídas: mapear o status direto no endpoint, duplicando a tabela que o
  handler global já usa, ou reaproveitar uma tabela única chamada dos dois lugares.

## Decisão

Um único `GlobalExceptionHandler`, implementando `IExceptionHandler`, captura tudo: validação de payload vira
`400`; exceptions de negócio, que herdam de `DomainException`, viram o status correspondente ao seu tipo de
erro; o inesperado vira `500`, sem vazar detalhe interno na resposta.

O `Result<T>` com falha, devolvido pelos handlers para outcomes esperados
([ADR-009](./009-guard-clauses-domain-exceptions.md)), não é exception — é convertido no endpoint, usando a
mesma tabela de tipo de erro para status HTTP. A tabela completa está em
[`error-handling.md`](../error-handling.md).

## Consequências

- Uma tabela só evita que o mapeamento se duplique entre o handler global e os endpoints. Mudar o status de um
  tipo de erro é alterar um único lugar.
- Em troca, a convenção depende de disciplina: todo código novo precisa sinalizar erro por exception de domínio
  ou por `Result<T>`, e não devolvendo `null` ou lançando exception genérica. Não há nada no compilador que
  force isso.

## Referências

- [RFC 7807 — Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc7807)
- [ASP.NET Core — tratamento de erro](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
