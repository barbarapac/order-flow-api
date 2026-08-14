# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## O que é este projeto

API de gestão de Pedidos (OrderFlow) em .NET 10 / ASP.NET Core Minimal API, construída como desafio técnico.
DDD com camadas `Domain` / `Application` / `Infrastructure` / `WebApi`, organizadas internamente por
**Vertical Slice** (a pasta raiz dentro de cada camada é a feature, não a camada técnica), EF Core + Postgres,
CQRS via `Mediator` (pacote de Martin Othamar, baseado em source generator — não confundir com `MediatR`), com
`Command`/`Query` distinguidos por `ICommand<TResponse>`/`IQuery<TResponse>`.

O código (entidades, propriedades, comandos: `User`, `Order`, `Product`, `Register`, `Confirm`, `Cancel`) é em
**inglês**; a documentação (`docs/*.md`, este arquivo) é em **português**.

Documentação de arquitetura completa — leia antes de decisões estruturais não triviais:
- `docs/domain-model.md` — agregados, invariantes, máquina de estados, eventos de domínio
- `docs/decisions.md` — ADRs numerados (ADR-001 a ADR-011) com o "porquê" de cada decisão + roadmap de fases
- `docs/error-handling.md` — taxonomia de erros e mapeamento para `ProblemDetails`

**Status atual**: Fases 1 e 2 concluídas (cadastro/login de usuário, JWT, tratamento de erro global; CRUD completo
de `Product`). `Order` (Fases 3–6 do roadmap em `docs/decisions.md`) ainda não existe no código.

## Comandos

```bash
# Build
dotnet build

# Testes (todos)
dotnet test

# Um teste específico
dotnet test --filter "FullyQualifiedName~UserTests.MethodName"

# Subir Postgres + API via Docker (migrations aplicadas automaticamente no startup)
docker compose up --build

# Só o Postgres (para rodar a API localmente fora do Docker)
docker compose up postgres -d
dotnet run --project src/OrderFlow.WebApi

# Nova migration EF Core (executar de dentro de src/OrderFlow.Infrastructure)
dotnet ef migrations add <Nome> --startup-project ../OrderFlow.WebApi
```

Swagger UI em `http://localhost:8080/swagger` (Docker) quando `ASPNETCORE_ENVIRONMENT=Development`.

## Arquitetura

### Camadas e dependências

`Domain` ← `Application` ← `Infrastructure` ← `WebApi` (referências de projeto seguem essa direção; `Domain`
não depende de nada, `Infrastructure` implementa interfaces definidas em `Application`/`Domain`).

Cada camada tem seu próprio `IoC.cs` com um método de extensão (`AddApplication`, `AddInfrastructure`,
`AddWebApi`) registrado em `Program.cs`.

### Vertical Slice dentro de cada camada

Dentro de `Domain`/`Application`/`WebApi`, a estrutura de pastas é por feature, não por tipo técnico:
`Users/Register/`, `Auth/Login/` (e futuramente `Products/Create/`, `Orders/Confirm/`, etc.), cada uma com seus
próprios Command/Handler/Validator/Response (Application) e Endpoint/Request (WebApi). Pastas `_Shared/` dentro
de cada camada guardam o que é genuinamente cross-cutting (Shared Kernel, `IEndpoint`, exception handler, etc.).

### Registro de endpoints (Minimal API)

Endpoints não são mapeados manualmente em `Program.cs`. Cada endpoint implementa `IEndpoint`
(`src/OrderFlow.WebApi/_Shared/IEndpoint.cs`) com um método `Map(IEndpointRouteBuilder)`; `AddEndpoints(assembly)`
descobre todas as implementações via reflection e `MapEndpoints()` as registra no startup. Para adicionar um
endpoint novo, basta criar a classe implementando `IEndpoint` na pasta da feature — não é preciso tocar em
`Program.cs`.

### Tratamento de erro — três caminhos, uma tabela

Ver `docs/error-handling.md` para o detalhe completo. Resumo do que importa ao escrever código novo:

- **Validação de payload** (FluentValidation): um `Validator` por Command, descoberto automaticamente
  (`AddValidatorsFromAssembly`). Roda no `ValidationBehavior` (`IPipelineBehavior<TMessage,TResponse>` do
  `Mediator`), **antes** do handler —
  lança `FluentValidation.ValidationException` → 400.
- **Invariante de agregado** (`Domain`): Guard Clauses estáticas (`UserGuard`, futuramente `OrderGuard`)
  chamadas no início de construtores/factories/métodos de transição, lançando `DomainException(code, message,
  ErrorType)` — fail fast, nunca deixa o objeto existir em estado inválido. Só agregados ricos (`User`, `Order`)
  têm Guard; `Product` é anêmico por exigência do enunciado e não tem.
- **"Não encontrado" / orquestração na Application**: `Result<T>` (`Result.Success`/`Result.Failure(Error)`),
  tratado direto no endpoint via `.ToProblemResult()` — nunca vira exception.
- Toda exception não tratada por `AppException` cai no `GlobalExceptionHandler` (`IExceptionHandler`, não
  middleware customizado), que usa a tabela única `ErrorTypeExtensions.ToStatusCode()`
  (`Validation`→400, `NotFound`→404, `Conflict`→409, `BusinessRule`→422) — a mesma tabela usada pelo
  `Result<T>.Failure` no endpoint. Não crie um novo mapeamento de status em outro lugar.
- Códigos de erro (`Code` em `AppException`/`Error`) seguem `{aggregate}.{motivo}` em snake_case
  (`order.invalid_transition`, `user.invalid_email`).

### Decisões de domínio a respeitar em código novo

- IDs de agregado são `Guid` gerados em memória (não `IDENTITY`/serial) — ver ADR-001.
- `Order.CustomerId` é sempre derivado do claim do JWT autenticado, nunca recebido no payload — não existe
  entidade `Customer` separada de `User` (ADR-003).
- Baixa de estoque só acontece em `Order.Confirm()`, nunca em `Order.Place()` — `Place` apenas valida
  `AvailableQuantity >= Quantity` (ADR-004). Confirmação usa update condicional (`WHERE available_quantity >=
  @qty`) dentro de transação + lock distribuído Redis por `ProductId` (ordenado, para evitar deadlock em pedidos
  multi-item) — ver ADR-005 e a seção 7 de `docs/domain-model.md` antes de mexer nesse fluxo.
- `Product` é deliberadamente anêmico (propriedades públicas, CRUD simples via Application); `Order`/`OrderItem`
  concentram todo o comportamento de negócio no agregado. Não adicione Guards/métodos de negócio em `Product`.
- `Order.Confirm()`/`Cancel()` são idempotentes por design (no-op silencioso se já no estado alvo, sem levantar
  evento de novo) — preserve esse comportamento ao alterar a máquina de estados.
- Senha em texto puro nunca chega ao Domain: validação de política de senha é FluentValidation na Application;
  o Domain só recebe o hash já pronto (`IPasswordHasher` fica na Infrastructure).

### Convenções de projeto

- Nullable + ImplicitUsings habilitados em todos os projetos (.NET 10).
- Testes usam xUnit + FluentAssertions + Moq + AutoBogus, espelhando a estrutura de pastas de `src/` dentro de
  `test/OrderFlow.UnitTest/` (`Domain/Users/`, `Application/Auth/`, `Infrastructure/Security/`, etc.).
