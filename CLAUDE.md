# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## O que é este projeto

API de gestão de pedidos (OrderFlow) em .NET 10 / ASP.NET Core Minimal API. DDD com as camadas
`Domain` / `Application` / `Infrastructure` / `WebApi`, cada uma organizada internamente por **Vertical Slice**
(a pasta de primeiro nível dentro da camada é a feature, não o tipo técnico). EF Core + PostgreSQL na escrita,
Dapper na leitura, Redis para lock distribuído, CQRS via `Mediator` (pacote de Martin Othamar, baseado em
source generator — **não** é o `MediatR`), com `ICommand<TResponse>` / `IQuery<TResponse>` distinguindo
escrita de leitura.

O código (entidades, comandos, propriedades: `User`, `Order`, `Product`, `Register`, `Confirm`, `Cancel`) é em
**inglês**; a documentação (`README.md`, `docs/*.md`, este arquivo) e as mensagens de erro ao usuário são em
**português** (ADR-002).

Documentação — leia antes de decisões estruturais não triviais:

- `docs/architecture.md` — camadas, Vertical Slice, caminho da requisição, escrita vs. leitura
- `docs/domain-model.md` — agregados, invariantes, máquina de estados, eventos
- `docs/concurrency.md` — o problema de estoque, lock distribuído + update condicional, limitações
- `docs/error-handling.md` — taxonomia de erros e mapeamento para `ProblemDetails`
- `docs/decisions/` — ADRs numerados (001 a 015), um por arquivo, com o "porquê" de cada escolha

## Comandos

```bash
dotnet build                                            # build da solution
dotnet test                                             # todos os testes
dotnet test --filter "FullyQualifiedName~OrderTests"    # um arquivo/classe de teste

docker compose up --build                               # Postgres + Redis + API (migrations no startup)
docker compose up postgres redis -d                     # só a infra, para rodar a API local
dotnet run --project src/OrderFlow.WebApi

# nova migration EF Core (executar de dentro de src/OrderFlow.Infrastructure)
dotnet ef migrations add <Nome> --startup-project ../OrderFlow.WebApi
```

Swagger UI em `http://localhost:8080/swagger` (Docker) — só é exposto quando
`ASPNETCORE_ENVIRONMENT=Development`.

O CI (`.github/workflows/ci.yml`) roda build + testes com cobertura OpenCover e SonarCloud com
`qualitygate.wait=true` — a cobertura é medida **apenas** sobre `[OrderFlow.Domain]` e `[OrderFlow.Application]`
(`/p:Include` no `dotnet test`, mais `sonar.coverage.exclusions`). Código novo em `WebApi`/`Infrastructure` não
precisa de teste para o gate passar; código novo em `Domain`/`Application` precisa.

## Arquitetura

### Camadas e composição

`Domain` ← `Application` ← `Infrastructure` ← `WebApi`. O `Domain` não depende de nada (exceção consciente:
`Mediator.Abstractions`, porque eventos de domínio são notificações do mediador). Interfaces de repositório são
declaradas no `Domain`; `IDistributedLock`, `IQueryExecutor`, `IUnitOfWork`, `IPasswordHasher` e
`IJwtTokenGenerator` na `Application` — todas implementadas na `Infrastructure`.

Cada camada tem um `IoC.cs` com um método de extensão (`AddApplication`, `AddInfrastructure`, `AddWebApi`),
chamado no `Program.cs`.

### Registro de endpoints

Endpoints não são mapeados à mão. Cada um implementa `IEndpoint` (`WebApi/_Shared/IEndpoint.cs`) com
`Map(IEndpointRouteBuilder)`; `AddEndpoints(assembly)` descobre as implementações por reflection e
`MapEndpoints()` registra no startup. **Adicionar um endpoint é criar uma classe na pasta da feature — o
`Program.cs` não muda.**

O endpoint só traduz HTTP em Command/Query e o `Result<T>` de volta em resposta (`.ToProblemResult()` na falha).
Nenhuma regra de negócio vive ali. O `CustomerId` sempre vem do claim do JWT (`user.GetUserId()`), nunca do
payload.

### Tratamento de erro — três caminhos, uma tabela

- **Validação de payload**: um `Validator` FluentValidation por Command, descoberto por
  `AddValidatorsFromAssembly`. Roda no `ValidationBehavior` (`IPipelineBehavior`) **antes** do handler; lança
  `ValidationException` → 400. Pipeline behaviors precisam ser registrados explicitamente no `AddMediator` —
  essa lib não faz assembly scan para eles.
- **Invariante de agregado** (`Domain`): Guard Clauses estáticas `internal` (`UserGuard`, `OrderGuard`) chamadas
  no início de construtores/factories/transições, lançando a exception selada do agregado (`OrderException`,
  `UserException`, ambas herdando de `DomainException`). Códigos e mensagens ficam em factories nomeadas dentro
  dessa exception, não espalhados pelos guards.
- **"Não encontrado" e orquestração** (`Application`): `Result<T>.Success` / `Result<T>.Failure(Error)`, tratado
  no endpoint — nunca vira exception.
- Toda exception não tratada cai no `GlobalExceptionHandler` (`IExceptionHandler`, não middleware), que usa a
  tabela única `ErrorTypeExtensions.ToStatusCode()` (`Validation`→400, `Unauthorized`→401, `NotFound`→404,
  `Conflict`→409, `BusinessRule`→422) — a mesma usada pelo `Result` no endpoint. **Não crie outro mapeamento de
  status em lugar nenhum.**
- Códigos de erro (`Code`) seguem `{agregado}.{motivo}` em snake_case: `order.not_found`,
  `order.invalid_transition`, `user.invalid_email`.

### Escrita e leitura

Escrita usa EF Core (change tracking, transações via `IUnitOfWork`). Leitura usa Dapper via `IQueryExecutor`,
com o SQL literal num `internal static class Sql` dentro da pasta da própria Query, projetando direto no
Response. As colunas são snake_case (`unit_price`, `available_quantity`, `created_at_utc`), exceto `"Id"`, que
é PascalCase e **precisa de aspas** no SQL — ver `ProductConfiguration`/`OrderConfiguration` para o mapeamento
real antes de escrever query nova.

## Decisões de domínio a respeitar em código novo

- IDs de agregado são `Guid` gerados em memória, `ValueGeneratedNever()` (ADR-001).
- Não existe entidade `Customer` separada de `User`; `Order.CustomerId` é o id do usuário autenticado (ADR-004).
- **Estoque é validado na criação do pedido e baixado só na confirmação** (ADR-005). A confirmação é a região
  crítica: `ConfirmOrderCommandHandler` pega o lock `order:{id}:status`, abre transação e publica o evento;
  `OrderConfirmedEventHandler` pega os locks `product:{id}:stock` **em ordem crescente de `ProductId`** (evita
  deadlock em pedido multi-item) e chama `DecrementStockAsync`, que é um update condicional
  (`WHERE available_quantity >= @qty` via `ExecuteUpdateAsync`). `affectedRows == 0` significa estoque
  insuficiente → `InsufficientStockException` derruba a transação inteira; não há baixa parcial. Leia
  `docs/concurrency.md` + ADR-006/ADR-007 antes de mexer nesse fluxo.
- Quem garante a invariante é o banco (o update condicional); o Redis apenas serializa antes. Se o Redis cair, a
  não-negatividade continua protegida.
- `Product` é deliberadamente anêmico (ADR-008): propriedades públicas, CRUD simples na Application, sem Guard e
  sem método de negócio. `Order`/`OrderItem` concentram o comportamento.
- As transições **retornam** o evento: `Confirm()` → `OrderConfirmed`, `Cancel()` → `OrderCanceled?` (`null`
  quando o pedido só estava `Placed`, pois não há estoque a devolver). Não existe `AggregateRoot.Raise()` nem
  lista interna de eventos — quem publica é o handler, dentro da transação.
- Idempotência de `Confirm`/`Cancel` mora **no handler** (early return em `IsConfirmed`/`IsCanceled`, dentro do
  lock), não no agregado — `OrderGuard.CanConfirm`/`CanCancel` continuam lançando em transição realmente
  inválida (→ 409). Preserve essa divisão.
- Senha em texto puro nunca chega ao `Domain`: a política de senha é FluentValidation na Application e o
  `Domain` só recebe o hash pronto.
- `Cors:AllowedOrigins` é obrigatório fora de `Development` — a ausência derruba o startup de propósito
  (ADR-015). Origens são normalizadas (barra final removida) em `CorsPolicies.Normalize`.

## Convenções de teste

`test/OrderFlow.UnitTest/` referencia **apenas** `Domain` e `Application` e espelha a estrutura de pastas de
`src/`. xUnit + FluentAssertions + Moq + AutoBogus.

Cada slice de teste segue o mesmo padrão: a classe de teste herda de um `Fixtures/<Handler>Fixture` que monta os
mocks e o handler; os dublês ficam em `Mocks/<Dependência>Mock.cs` (wrappers sobre `Mock<T>` expondo
`ConfigureXToReturn` / `VerifyX`); os dados em `Fakers/<Tipo>Faker.cs`. Testes seguem
`Metodo_Cenario_Resultado` com blocos `// Arrange` / `// Act` / `// Assert`. Ao criar um teste novo, siga o
formato do slice vizinho em vez de instanciar `Mock<T>` direto no teste.
