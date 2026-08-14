# Decisões técnicas — OrderFlow

Registro de decisões de arquitetura (estilo ADR curto) tomadas durante o desafio. Para o modelo de domínio em
detalhe, ver [`domain-model.md`](./domain-model.md); para o mecanismo de tratamento de erro, ver
[`error-handling.md`](./error-handling.md).

**Nota de idioma**: o texto destes documentos está em português. Os nomes de classes, propriedades e métodos no
código (`User`, `Order`, `Product`, `Place`, `Confirm`, `Cancel`...) ficam em **inglês** — ver ADR-002.

## ADR-001 — IDs de agregado como `Guid`

**Decisão**: `User`, `Product` e `Order` usam `Guid` como Id, gerado em memória na criação (não `IDENTITY`/serial
do banco).
**Motivo**: evita enumeração sequencial de recursos (IDOR) em uma API pública, e permite ao Domain gerar um Id
válido antes de persistir (útil para publicar eventos de domínio com o Id definitivo desde o primeiro momento).
**Trade-off aceito**: índice um pouco maior que `int`/`bigint` no Postgres; irrelevante no volume deste desafio.

## ADR-002 — Domínio nomeado em inglês, seguindo a linguagem ubíqua do enunciado

**Decisão**: entidades, propriedades e regras de negócio são nomeadas em **inglês**
(`User`, `Order`, `Product`, `Register`, `Confirm`, `Cancel`), diferente do `fiap-fcg-user-api` (português —
`Usuario`, `Cadastrar`). A documentação (este arquivo, `domain-model.md`, `error-handling.md`) é escrita em
**português**.
**Motivo**: o enunciado do desafio já define a linguagem ubíqua em inglês para as entidades e os endpoints
(`POST /orders/{id}/confirm`, `Status`, `CustomerId`). Seguir a nomenclatura dada evita ambiguidade de tradução e
mantém o código citável 1:1 com o PDF do desafio — quem avaliar consegue mapear código direto pro enunciado sem
etapa mental de tradução.

## ADR-003 — Não existe entidade `Customer` separada

**Decisão**: `Order.CustomerId` é o `User.Id` do usuário autenticado (extraído do claim `sub`/`NameIdentifier` do
JWT). Não há cadastro de cliente distinto do cadastro de usuário.
**Motivo**: o enunciado não lista `Customer` entre as entidades sugeridas; introduzir uma sem necessidade seria
over-engineering para o escopo.
**Implicação**: o endpoint `POST /orders` não recebe `customerId` no payload apesar do PDF sugerir isso no
"payload mínimo" — ele é derivado do token, o que é mais seguro (impede um usuário criar pedido em nome de
outro). Esse desvio do payload sugerido será destacado no README como decisão consciente.

## ADR-004 — Baixa de estoque só na confirmação, não na criação

**Decisão**: `POST /orders` (nasce `Placed`) apenas *valida* `AvailableQuantity >= Quantity`, sem decrementar.
`POST /orders/{id}/confirm` é quem decrementa de fato. `POST /orders/{id}/cancel` só devolve estoque se o pedido
já estava `Confirmed`.
**Motivo**: evita que pedidos `Placed` e nunca confirmados fiquem "prendendo" estoque indefinidamente sem um
mecanismo de expiração. Também concentra toda a lógica de concorrência crítica em um único ponto de entrada
(`Confirm`), simplificando o raciocínio sobre onde o lock/transação precisa existir.
**Consequência aceita**: é possível `Place` um pedido que, no momento do `Confirm`, não tenha mais estoque
suficiente (por outro pedido concorrente confirmado primeiro) — nesse caso `Confirm` retorna `409 Conflict` e o
pedido permanece `Placed` (cliente decide se cancela ou tenta depois).

## ADR-005 — Concorrência: evento de domínio + lock distribuído (Redis) + update condicional transacional

**Decisão**: `Order.Confirm()` apenas transiciona o status e levanta `OrderConfirmedDomainEvent`. Um
`INotificationHandler` (`Mediator`) consome o evento, adquire um lock distribuído no Redis por
`product:{productId}:stock` (ProductIds ordenados para evitar deadlock em pedidos multi-item) e executa, dentro
da mesma transação Postgres do `ConfirmOrderCommandHandler`, um `UPDATE ... WHERE available_quantity >= @qty`
verificando linhas afetadas.
**Motivo**: o enunciado aceita tanto RowVersion otimista quanto update atômico condicional; optamos pelo update
condicional porque é o que realmente impede estoque negativo no banco. O lock distribuído via Redis é uma camada
adicional, pensada para quando a API rodar em múltiplas réplicas — serializa a seção crítica antes mesmo de tocar
o banco e demonstra um padrão que se estende bem se o controle de estoque um dia sair para um serviço externo.
**Detalhe de implementação**: o dispatch do evento acontece **antes** do commit da transação que persiste
`Order.Status = Confirmed` — se o update de estoque falhar (linhas afetadas = 0) para qualquer item, uma
`AppException` (`InsufficientStockException`, `ErrorType.Conflict`) é lançada, a transação inteira sofre rollback
(nenhum produto do pedido fica parcialmente decrementado) e o middleware global de exceções traduz isso em
`409 Conflict` via `ProblemDetails`. Essa exception **não é uma `DomainException`** (não nasce de um Guard dentro
do agregado `Order`/`Product`) — é uma `AppException` irmã, lançada pelo handler de aplicação para controlar o
fluxo transacional; ver ADR-011 / [`error-handling.md`](./error-handling.md) para a taxonomia completa.
**Biblioteca**: implementação manual com `StackExchange.Redis` puro (sem `RedLock.net` nem
`Medallion.Threading.Redis`) — aquisição via `SET lock:{resource} {guid} NX EX 30` (`StringSetAsync` com
`When.NotExists`, atômico), espera por polling (`Task.Delay(50ms)`) enquanto a chave existir, e liberação via
script Lua atômico (`EVAL`/`ScriptEvaluateAsync`, `GET` + `DEL` condicional em uma única chamada) que só apaga a
chave se o valor armazenado ainda for o `Guid` gerado por aquela aquisição — evita que uma instância libere um
lock que já expirou e foi readquirido por outro processo.
**Motivo**: simplicidade e controle total sobre o comportamento. O projeto roda com uma única instância Redis
(sem cluster/sentinela), então o algoritmo Redlock multi-instância que `RedLock.net` implementa não se aplica
aqui — seria complexidade sem benefício real. `StackExchange.Redis` já expõe os primitivos necessários
(`StringSetAsync`, `ScriptEvaluateAsync`) e suas interfaces (`IConnectionMultiplexer`, `IDatabase`) são
diretamente mockáveis em teste, sem depender de um Redis real rodando.

## ADR-006 — `Product` anêmico, `Order` rico

**Decisão**: `Product` é um DTO-like com propriedades públicas e CRUD simples via Application (exigência
explícita do enunciado: "domínio anêmico"). `Order`/`OrderItem` concentram invariantes e comportamento
(`Place`, `Confirm`, `Cancel`) no próprio agregado.
**Motivo**: o enunciado pede explicitamente as duas coisas ao mesmo tempo — isso é proposital do desafio (avaliar
se sabemos reconhecer quando um domínio realmente precisa ser rico vs. quando complexidade adicional não traria
valor). Catálogo de produto aqui é só cadastro; pedido é onde vivem as regras de negócio reais.

## ADR-007 — Guard Clauses + Domain Exceptions no Domain; `Result<T>` só para orquestração na Application

**Decisão**: invariantes de agregado (construção e transição de estado) são validadas por **Guard Clauses**
estáticas (`UserGuard`, `OrderGuard`) chamadas no início de construtores/factories/métodos de domínio, que
lançam `DomainException` (com `Code` + `ErrorType`) assim que a invariante é violada — fail fast, o objeto nunca
chega a existir em estado inválido. `Result<T>` deixa de ser o mecanismo de erro do Domain e passa a cobrir só
outcomes de **orquestração na Application** que o próprio agregado não tem como avaliar sozinho — hoje, isso se
resume a "recurso não encontrado no repositório" (ex.: `PlaceOrderCommandHandler` consulta
`IProductRepository`, produto não existe → `Result.Failure(Error.NotFound(...))`, sem tocar em exception).
**Motivo da mudança**: `Result<T>` não funciona bem em construtores — um construtor/factory ou constrói um objeto
válido ou não constrói nada, não existe "meio-termo" que uma assinatura `Result<T>` conseguiria expressar de
forma limpa. Guard + exception resolve isso naturalmente, e ainda casa direto com o "fastfail + problem details
em tratamento global" que o enunciado pede como desejável.
**Escopo**: só `User` e `Order`/`OrderItem` (agregados ricos) usam Guard. `Product`, por ser anêmico (ADR-006),
não tem Guard — validação dele é 100% FluentValidation na Application.
**Resolvido pelo ADR-011**: o mecanismo exato de tradução dessas exceptions (e do `Result<T>`) para
`ProblemDetails` — antes um item em aberto neste ADR — está detalhado em ADR-011 e
[`error-handling.md`](./error-handling.md).

## ADR-008 — Organização por Vertical Slice dentro de cada camada

**Decisão**: `Domain`/`Application`/`Infrastructure`/`WebApi` continuam como projetos separados (limite de
compilação e de dependência), mas dentro de cada um a pasta raiz é a *feature*
(`Users/Register`, `Orders/Confirm`, `Products/Create`...), não a camada técnica.
**Motivo**: é literalmente o requisito não funcional do enunciado ("a separação de responsabilidades existe,
porém a organização deve ser por Vertical Slices"). É também o mesmo padrão já usado no `fiap-fcg-user-api`
(pastas `Usuarios/Cadastrar`, `Usuarios/Consultar/...` dentro de cada camada).

## ADR-009 — JWT com expiração de 60 minutos, sem refresh token nesta fase

**Decisão**: token emitido por `POST /auth/token` expira em 60 minutos (`DateTime.UtcNow.AddMinutes(60)`).
Refresh token fica fora do escopo do desafio.
**Motivo**: requisito explícito do time. 60 min é suficiente para o uso manual/Swagger durante a avaliação sem
reintroduzir login a cada teste.

## ADR-010 — Migrations aplicadas automaticamente no startup

**Decisão**: `app.Services.GetRequiredService<OrderFlowDbContext>().Database.MigrateAsync()` roda no boot da API
(atrás de uma flag de configuração, habilitada por padrão em Docker/Development).
**Motivo**: requisito explícito do enunciado ("Migrations aplicadas automaticamente" + `docker compose up` deve
subir API + banco prontos para uso). Para um cenário de produção real, a abordagem preferida seria um passo de
deploy separado (migration bundle) — mencionado aqui como trade-off consciente para o escopo do teste.

## ADR-011 — `IExceptionHandler` + `ValidationBehavior` + tabela única `ErrorType` → status HTTP

**Decisão**: tratamento de erro global via `IExceptionHandler` (mecanismo nativo do .NET 8+, não middleware
customizado), com três caminhos bem separados — exceptions de validação de formato (`FluentValidation`,
levantadas por um `ValidationBehavior` do `Mediator` antes de qualquer handler rodar), exceptions tipadas de
negócio
(`AppException` e suas derivadas `DomainException`/`InsufficientStockException`) e o inesperado (500, sem vazar
detalhe interno). Todas mapeadas por uma única tabela `ErrorType → HttpStatusCode`, reaproveitada também pelo
`Result<T>.Failure` retornado pelos handlers (não é exception, mapeado direto no endpoint). Detalhamento completo,
com exemplos de payload `ProblemDetails`, em [`error-handling.md`](./error-handling.md).
**Motivo**: `IExceptionHandler` é o padrão atual do framework para isso (substitui a `ExceptionMiddleware.cs`
customizada que o `fiap-fcg-user-api` usa, que é o padrão de .NET 6/7); centralizar a tabela de mapeamento evita
que a mesma lógica `ErrorType → status` se duplique entre o handler de exception e o código do endpoint que lida
com `Result<T>`.
**Resolve**: o item em aberto deixado no ADR-007.

---

## Roadmap de implementação

- [x] **Fase 0** — Esqueleto de solução (`Domain`/`Application`/`Infrastructure`/`WebApi`/`UnitTest`), .NET 10,
      Minimal API.
- [x] **Fase 1**: Shared Kernel (`Entity`, `AggregateRoot`, `DomainEvent`, `AppException`,
      `DomainException`, `Result`/`Error`); agregado `User` + `Email` VO + `UserGuard`; `OrderFlowDbContext` +
      configuração EF + migration inicial (Postgres); `IPasswordHasher` (BCrypt) + `IJwtTokenGenerator` (60 min);
      vertical slices `Users/Register` (`POST /users`) e `Auth/Login` (`POST /auth/token`); `GlobalExceptionHandler`
      (`IExceptionHandler`) + `ValidationBehavior` + `AddProblemDetails()`; Swagger com suporte a Bearer;
      `docker-compose` com Postgres — validado de ponta a ponta (`docker compose up`, migration aplicada
      automaticamente, registro/login/JWT/erros retornando o status HTTP correto).
- [x] **Fase 2** — CRUD de `Product` (anêmico): agregado `Product` sem Guards (propriedades públicas,
      `Id`/`CreatedAtUtc` gerados no próprio objeto); vertical slices `Products/Create`, `Products/Update`,
      `Products/Delete`, `Products/GetById`, `Products/GetAll` (`POST /products`, `PUT /products/{id}`,
      `DELETE /products/{id}`, `GET /products/{id}`, `GET /products`), todos atrás de `RequireAuthorization()`
      (qualquer usuário autenticado — ainda não há `Role`/Admin); validação 100% FluentValidation (nome
      obrigatório, `UnitPrice > 0`, `AvailableQuantity >= 0`); `ProductConfiguration` + migration `AddProduct` —
      validado de ponta a ponta contra Postgres real via Docker (CRUD completo, 404 após delete, 400 de
      validação).
- [x] **Fase 3** — `POST /orders` (`Place`): validação de itens/estoque, `Total`, `GET /orders/{id}`,
      `GET /orders` (paginação + filtros).
- [ ] **Fase 4** — `POST /orders/{id}/confirm`: evento de domínio, lock distribuído Redis, update condicional
      transacional, idempotência.
- [ ] **Fase 5** — `POST /orders/{id}/cancel`: idempotência, devolução condicional de estoque.
- [ ] **Fase 6** — Testes (xUnit): regras de domínio, casos de borda de concorrência, handlers de Application.
      Auditoria de `CancellationToken` end-to-end.
- [ ] **Fase 7** — README final, revisão de `docker compose up`, checklist do enunciado.
