# OrderFlow API

Serviço de gestão de Pedidos com itens, validação de estoque, autenticação JWT e operações idempotentes.

Construído em .NET 10 / Minimal API, com DDD (Domain / Application / Infrastructure / API) organizado por
**Vertical Slice**, EF Core + Postgres, e CQRS simplificado via MediatR.

Documentação de arquitetura e decisões técnicas:

- [`docs/domain-model.md`](docs/domain-model.md) — modelagem de domínio (agregados, invariantes, eventos)
- [`docs/decisions.md`](docs/decisions.md) — ADRs e roadmap de implementação
- [`docs/error-handling.md`](docs/error-handling.md) — tratamento de erro e `ProblemDetails`

## Status atual — Fase 1

Implementado até aqui: Shared Kernel (Guards, `DomainException`/`AppException`, `Result<T>`), agregado `User`,
persistência EF Core + Postgres com migration inicial, autenticação JWT (expiração de 60 min), cadastro de
usuário e login, middleware global de erro (`ProblemDetails`), Swagger com suporte a Bearer.

Os endpoints de `Product`/`Order` do enunciado ainda não foram implementados — ver o roadmap em
[`docs/decisions.md`](docs/decisions.md#roadmap-de-implementação).

## Stack

- .NET 10 · ASP.NET Core Minimal API
- EF Core 10 + Npgsql (Postgres)
- MediatR (CQRS) + FluentValidation (fastfail)
- BCrypt.Net (hash de senha) + JWT Bearer
- Swashbuckle (Swagger)
- xUnit + FluentAssertions + Moq

## Como rodar

### Via Docker (recomendado)

Requer Docker Desktop rodando.

```bash
docker compose up --build
```

Isso sobe Postgres + API. As migrations do EF Core são aplicadas automaticamente no startup da API
(ver [ADR-010](docs/decisions.md#adr-010-migrations-aplicadas-automaticamente-no-startup)). A API fica disponível em:

- Swagger UI: http://localhost:8080/swagger
- API: http://localhost:8080

Para derrubar: `docker compose down` (adicione `-v` para também apagar o volume do Postgres).

### Localmente (sem Docker)

Requer um Postgres acessível em `localhost:5432` (pode usar `docker compose up postgres -d` para subir só o
banco) e o .NET 10 SDK.

```bash
dotnet run --project src/OrderFlow.WebApi
```

A connection string e a chave de assinatura JWT de desenvolvimento já estão em
`src/OrderFlow.WebApi/appsettings.Development.json` (valores de uso local apenas, não são segredos reais).

### Testes

```bash
dotnet test
```

## Endpoints implementados (Fase 1)

### `POST /users` — cadastro de usuário

```bash
curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Doe","email":"jane@example.com","password":"S3cret123"}'
```

### `POST /auth/token` — login

```bash
curl -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"email":"jane@example.com","password":"S3cret123"}'
```

Retorna `{ "token": "...", "expiresAtUtc": "...", ... }`. Use o token no Swagger clicando em **Authorize** e
colando `Bearer {token}`, ou via header `Authorization: Bearer {token}` em chamadas subsequentes.

## Decisões relevantes (resumo)

- `CustomerId` do pedido é sempre o `Id` do usuário autenticado — não existe cadastro de cliente separado
  ([ADR-003](docs/decisions.md#adr-003-não-existe-entidade-customer-separada)).
- IDs de agregado são `Guid`, gerados em memória ([ADR-001](docs/decisions.md#adr-001-ids-de-agregado-como-guid)).
- Invariantes de domínio usam Guard Clauses + `DomainException`; `Result<T>` fica restrito a "não encontrado"
  na Application ([ADR-007](docs/decisions.md#adr-007-guard-clauses-domain-exceptions-no-domain-resultt-só-para-orquestração-na-application)).
- Tratamento de erro global via `IExceptionHandler` (.NET 8+), com uma única tabela `ErrorType → status HTTP`
  reaproveitada pelo `Result<T>` e pelas exceptions ([ADR-011](docs/decisions.md#adr-011--iexceptionhandler--validationbehavior--tabela-única-errortype--status-http)).

A lista completa de decisões, com o "porquê" de cada uma, está em [`docs/decisions.md`](docs/decisions.md).
