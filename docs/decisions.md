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
**Extensão — lock por pedido em `Confirm`/`Cancel`**: além do lock por produto (`product:{id}:stock`, adquirido
dentro do `NotificationHandler` para proteger a baixa/devolução de estoque), `ConfirmOrderCommandHandler` e
`CancelOrderCommandHandler` adquirem um segundo lock, `order:{orderId}:status`, logo no início do `Handle` —
antes até da leitura do pedido — e o mantêm (`await using`) até o fim do método. **Motivo**: a checagem de
idempotência (`order.IsConfirmed`/`order.IsCanceled`) é um read seguido de decisão ("já está nesse estado? não
faz nada") que só é seguro se nada mais puder transicionar o mesmo pedido entre a leitura e a escrita — sem esse
lock, duas requisições concorrentes para `Confirm` e `Cancel` do mesmo pedido poderiam ambas ler o estado antigo,
ambas decidir prosseguir, e disparar os dois `NotificationHandler`s de estoque (baixa E devolução) para o mesmo
pedido. Com o lock, a segunda requisição só lê o pedido depois que a primeira já fez commit/rollback e liberou o
lock, então ou vê o novo estado (idempotência funciona) ou ainda o antigo (nenhuma concorrência real havia).
Validado manualmente disparando `Confirm` e `Cancel` em paralelo para o mesmo pedido: a chamada que perde a
corrida do lock lê o estado já transicionado pela vencedora e recebe `409 order.invalid_transition` (via
`OrderGuard`) em vez de processar uma transição inválida.

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

## ADR-012 — Dapper no read-side das queries, SQL dentro do próprio `QueryHandler`; EF Core continua dono da escrita

**Decisão**: as 4 queries de leitura (`GetById`/`GetAll` de `Product` e `Order`) passam a usar Dapper direto
dentro do próprio `QueryHandler` (`Application`), sem repositório por agregado. Cada slice ganha uma classe
`Sql` interna (`Products/GetById/Sql.cs`, `Products/GetAll/Sql.cs`, `Orders/GetById/Sql.cs`,
`Orders/GetAll/Sql.cs`) que concentra os textos de SQL e a montagem de parâmetros — o `QueryHandler` fica só com
a orquestração (chamar `Sql.X` através de um `IQueryExecutor`, mapear o resultado). Cada handler recebe um
`IQueryExecutor` (definido na `Application`, três métodos genéricos — `QuerySingleOrDefaultAsync<T>`,
`QueryAsync<T>`, `QueryCountAndListAsync<T>` — todos recebendo `string sql` + `object? parameters`), implementado
por `DapperQueryExecutor` na `Infrastructure` (abre a conexão via `NpgsqlDataSource` e chama o Dapper). O
`QueryHandler` nunca toca em `DbConnection`/Dapper diretamente. `IProductRepository`/`IOrderRepository` (EF,
`Domain`) não mudam e continuam sendo a única via de acesso a dado para os Commands (`Place`/`Confirm`/`Cancel`
de `Order`, CRUD de `Product`).
**Motivo**: essas 4 queries são pass-through simples — o `QueryHandler` só busca e devolve, sem lógica de
negócio no meio. Um repositório por agregado (`IProductReadRepository`/`ProductReadRepository`, tentado antes
deste ADR) criava uma interface com exatamente uma implementação possível, só para mover a mesma SQL uma camada
pra baixo; sem outro consumidor nem outra implementação prevista, a indireção não pagava seu custo. `Order`
tem `Items` (1-N) e dois campos calculados em memória (`Order.Total`, `OrderItem.LineTotal`, nenhum é coluna
persistida) — a lógica de agrupamento fica no mesmo arquivo do caso de uso que a consome, sem espalhar por duas
camadas.
**Trade-off aceito conscientemente — SQL na `Application`**: isso quebra o isolamento que o projeto mantém em
todo o resto do código, onde só a `Infrastructure` sabe que o banco é Postgres (EF configs, connection string,
sintaxe SQL). As classes `Sql` de cada slice citam sintaxe específica do dialeto (`OFFSET/LIMIT`, `ANY(@array)`)
e o projeto referencia o pacote `Dapper` diretamente também no `OrderFlow.Application.csproj` (não só no
`Infrastructure`). Escolha deliberada em favor de menos indireção — ver "alternativas descartadas" abaixo.
**Teste unitário via `IQueryExecutor` mockável**: os métodos do Dapper (`QueryAsync`, `QueryMultipleAsync`) são
extension methods sobre `DbConnection`/`IDbConnection` — não são mockáveis com Moq (tentam executar de verdade
contra a conexão). Por isso o handler nunca chama Dapper diretamente; ele depende só de `IQueryExecutor`, uma
interface comum (não específica de agregado) que Moq mocka normalmente. Os testes dos 4 `QueryHandler`s mockam
`IQueryExecutor.QuerySingleOrDefaultAsync<TResponse>`/`QueryAsync<TResponse>`/`QueryCountAndListAsync<TResponse>`
com o tipo de retorno específico de cada handler (Moq suporta setup de método genérico fechado num tipo
concreto), sem se importar com o texto exato da SQL passada — o que valida é a orquestração do handler
(mapeamento, cálculo de `Total`/`LineTotal`, tratamento de "não encontrado"), não a query em si. Isso exige que
`OrderItemRow` (o tipo auxiliar de `GetAllOrdersQueryHandler` que carrega `OrderId` pra agrupar itens por
pedido) seja `public`, não `private`/`internal` — o mock do teste precisa poder nomear esse tipo genérico
(`QueryAsync<OrderItemRow>`) de fora do assembly `Application`. Cogitou-se `internal` + `InternalsVisibleTo`
pra "esconder" o tipo do resto do mundo mantendo-o visível só pro teste, mas isso não protege invariante
nenhuma (é um DTO plano, sem comportamento, igual a todo `Response`/`*Row` do projeto) — só adicionava uma
camada de configuração pra simular um encapsulamento que não fazia falta. `public` simples, sem
`InternalsVisibleTo`/`AssemblyInfo.cs`, resolve com menos peça móvel.
**Alternativas descartadas**: (1) repositório por agregado na `Application` retornando `Response` — funciona,
mas ceremony sem benefício real pra queries pass-through; (2) sem teste algum pra esses 4 handlers, validação só
manual via Docker — rejeitado porque o objetivo é ter cobertura automatizada com validação real dos campos do
`Response`, não só uma checagem de fumaça; (3) testes de integração com Testcontainers (Postgres real efêmero por
teste) — cobre o SQL de verdade, mas exige Docker rodando durante `dotnet test` e um projeto de teste separado;
descartado porque o requisito era manter testes unitários rápidos com mock, como o resto do projeto; (4) handler
dependendo direto da classe concreta da `Infrastructure` (sem interface) — evita interface só pra viabilizar
mock, mas viola a direção de dependência do projeto (`Application` nunca referencia `Infrastructure`) e não
resolveria a testabilidade mesmo assim (a classe concreta ainda chamaria Dapper por baixo).
**Sem conexão/transação compartilhada com o EF**: nenhuma dessas 4 queries participa de transação — não há
motivo pra `IQueryExecutor`/`DapperQueryExecutor` compartilhar `DbConnection`/`DbTransaction` com o
`OrderFlowDbContext`. `DapperQueryExecutor` abre uma conexão nova por chamada via `NpgsqlDataSource` (singleton,
`NpgsqlDataSourceBuilder` sobre a mesma `connectionString` do `OrderFlowDb`), independente do ciclo de vida do
`DbContext`/`IUnitOfWork`. Se um dia surgir uma query de leitura dentro de um fluxo transacional, essa decisão
precisa ser revisitada.
**Mapeamento de nomes**: cada `SELECT` aliasa explicitamente toda coluna `snake_case` pro nome `PascalCase` do
`Response`/record correspondente (`unit_price AS UnitPrice`, `created_at_utc AS CreatedAtUtc`...) — sem depender
de `Dapper.DefaultTypeMap.MatchNamesWithUnderscores` (configuração global mutável, setada uma vez no startup e
válida pra todo o processo). O alias explícito é redundante em texto, mas autodocumenta a query e não depende de
nenhum estado fora do próprio `SELECT`.
**Filtro condicional de `status` (`Orders/GetAll`)**: `Sql.Parameters` monta um `DynamicParameters` do Dapper e
só adiciona o parâmetro `@Status` quando `request.Status` não é nulo; o `WHERE` correspondente
(`Sql.CountAndPage`) só concatena `AND status = @Status` nesse caso. Substitui uma primeira versão que sempre
passava `@Status` (possivelmente `null`) e usava `(@Status::text IS NULL OR status = @Status)` no SQL — o cast
`::text` era necessário só porque o Postgres não consegue inferir o tipo de um parâmetro `NULL` sem tipagem
explícita; com o parâmetro condicionalmente ausente, o problema desaparece.
**Paginação**: `QueryMultipleAsync` executa o `COUNT(*)` e a página (`OFFSET`/`LIMIT`) em uma única ida ao banco
— uma melhoria sobre o EF atual, que fazia `CountAsync` + `ToListAsync` como dois round-trips separados.
**`Order`/`OrderItem` — 2 queries + agrupamento em memória, não JOIN**: `Order.Total` e `OrderItem.LineTotal` são
recalculados em C# (`Sum(UnitPrice * Quantity)`) depois de buscar os itens. Em `GetAllOrdersQueryHandler`, os
itens (`order_items`) são buscados numa query separada da página de `orders` (`WHERE order_id = ANY(@OrderIds)`,
sintaxe nativa de array do Postgres/Npgsql) e agrupados em memória por `OrderId` — um `JOIN orders/order_items`
quebraria o `OFFSET/LIMIT` da paginação (a contagem de linhas deixa de corresponder a pedidos) e duplicaria a
linha do pedido por item, exatamente o problema que `AsSplitQuery()` já existia para evitar no EF.
`GetOrderByIdQueryHandler` busca um único pedido, então dispensa o agrupamento por `OrderId`.
**`Response` records com `init` + `with` em vez de construtor posicional, pra mapear direto sem DTO de linha
intermediário**: `GetProductByIdResponse`, `GetAllProductsResponse`, `GetOrderByIdResponse`/`GetOrderByIdItemResponse`
e `GetAllOrdersResponse`/`GetAllOrdersItemResponse` trocaram de record posicional pra `{ get; init; }` com
valores default (`Items` = `[]`, `Total`/`LineTotal` = `0`). Isso muda o modo de binding do Dapper: em vez de
casar colunas com parâmetros de um construtor (que exige *todas* as colunas presentes), o Dapper instancia via
construtor sem parâmetros e preenche só as propriedades que batem com uma coluna retornada, deixando o resto no
default declarado. Na prática, isso deixou `GetOrderByIdQueryHandler` mapear a query do cabeçalho **direto** pra
`GetOrderByIdResponse` (sem precisar do `OrderHeaderRow` que a primeira versão deste ADR usava) e depois compor o
resultado final com `header with { Total = ..., Items = ... }`; o mesmo vale pra `GetAllOrdersQueryHandler`
(`GetAllOrdersResponse` direto, sem `OrderHeaderRow`). `OrderItemRow` (`internal`, top-level no namespace
`Orders.GetAll` — não mais aninhado `private` dentro do handler, pra o teste conseguir referenciá-lo via
`InternalsVisibleTo`) continua existindo — ele carrega `OrderId`, que não faz parte do contrato público
(`GetAllOrdersItemResponse` não expõe `OrderId`), então ainda é preciso pra agrupar os itens por pedido antes de
descartar esse campo.
**Trade-off aceito**: perde-se a garantia do compilador de que todo campo do `Response` foi preenchido —
com construtor posicional, esquecer `Total`/`Items` era erro de compilação; com `init` + default, um caminho de
código que devolvesse o objeto sem compor os itens retornaria silenciosamente `Total = 0`/`Items = []` em vez de
falhar a build. Aceito porque cada handler tem um único caminho linear até o `return`, sem ramificação que
arrisque devolver o objeto incompleto por engano.
**Pegadinha de schema encontrada na validação manual**: as colunas de PK (`Id`) de `products`/`orders`/
`order_items` ficaram como `"Id"` (PascalCase, case-sensitive) no Postgres, não `id` — nenhuma `Configuration`
(`ProductConfiguration`/`OrderConfiguration`) chama `HasColumnName` para a PK, só para as demais colunas
(`unit_price`, `customer_id`...), então o EF preservou o case original da propriedade C# só nesse caso. Todo SQL
Dapper que referencia a PK precisa citar `"Id"` entre aspas duplas (`SELECT "Id", ... WHERE "Id" = @Id`);
esquecer as aspas resulta em `42703: column "id" does not exist` do Postgres — reproduzido e corrigido durante a
validação manual contra o Postgres real (`docker compose up` + `dotnet run` local).

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
- [x] **Fase 4** — `POST /orders/{id}/confirm`: `Order.Confirm()` idempotente levanta `OrderConfirmedDomainEvent`;
      `ConfirmOrderCommandHandler` abre transação Postgres, publica o evento via `IPublisher` e faz commit/rollback
      conforme o resultado; `OrderConfirmedDomainEventHandler` adquire locks distribuídos Redis por
      `product:{id}:stock` (ProductIds ordenados) e roda `IProductRepository.DecrementStockAsync` (`UPDATE ...
      WHERE available_quantity >= @qty` via `ExecuteUpdateAsync`) para cada item; 0 linhas afetadas lança
      `InsufficientStockException` (`Application`, `ErrorType.Conflict`) e reverte a transação inteira — validado
      de ponta a ponta contra Postgres + Redis reais via Docker (confirmação simples, idempotência, 404, e disputa
      de estoque concorrente entre dois pedidos `Placed` do mesmo produto).
- [x] **Fase 5** — `POST /orders/{id}/cancel`: `Order.Cancel()` idempotente (no-op silencioso se já `Canceled`,
      via `OrderGuard.CanCancel`), válido a partir de `Placed` ou `Confirmed`; só retorna `OrderCanceledDomainEvent`
      (para devolução de estoque) quando a transição parte de `Confirmed` — de `Placed` não há nada a devolver, o
      método retorna `null` e `CancelOrderCommandHandler` simplesmente não publica evento; `OrderCanceledEventHandler`
      adquire os mesmos locks distribuídos Redis por `product:{id}:stock` (ProductIds ordenados) e roda
      `IProductRepository.IncrementStockAsync` (`UPDATE ... SET available_quantity += @qty`, sem condição — devolução
      de estoque não tem risco de ficar negativo) para cada item; `ConfirmOrderCommandHandler` e
      `CancelOrderCommandHandler` também adquirem um lock distribuído por pedido (`order:{orderId}:status`, ver
      extensão do ADR-005) que protege a checagem de idempotência + transição contra `Confirm`/`Cancel` concorrentes
      no mesmo pedido — validado de ponta a ponta contra Postgres + Redis reais via Docker (cancelamento de `Placed`
      sem alterar estoque, cancelamento de `Confirmed` devolvendo estoque, idempotência sem devolução duplicada,
      404 para pedido inexistente, e `Confirm`/`Cancel` disparados em paralelo no mesmo pedido sendo serializados
      pelo lock).
- [x] **Fase 6** — Testes (xUnit): 138 testes cobrindo regras de domínio (`Order`/`OrderGuard`, incluindo a
      transição inválida `Confirm` a partir de `Canceled`), handlers de Application (`Place`/`Confirm`/`Cancel`,
      CRUD de `Product`, `Register`/`Login`), casos de borda de concorrência (idempotência de `Confirm`/`Cancel`,
      corrida `Cancel` vencendo `Confirm` no mesmo pedido, falha parcial de estoque multi-item com liberação de
      todos os locks, polling/cancelamento do `RedisDistributedLock`) e infraestrutura (`JwtTokenGenerator`,
      `PasswordHasher`, `ValidationBehavior`). Auditoria de `CancellationToken` end-to-end em todos os
      handlers/repositórios/endpoints: propagação correta confirmada em toda a cadeia; **um problema real
      encontrado e corrigido** — `ConfirmOrderCommandHandler`/`CancelOrderCommandHandler` faziam
      `RollbackTransactionAsync(cancellationToken)` dentro do `catch` reaproveitando o token da requisição; se o
      cliente já tivesse desconectado (token cancelado) no momento da falha, o rollback lançava
      `OperationCanceledException` antes de tocar o banco, mascarando a exception original e deixando a transação
      aberta. Corrigido usando `CancellationToken.None` explicitamente nesse `catch` — rollback é limpeza
      obrigatória e não deve respeitar o cancelamento que originou a falha — com teste de regressão para os dois
      handlers.
- [x] **Dapper no read-side** — as 4 queries (`GetById`/`GetAll` de `Product`/`Order`) migradas de EF Core para
      Dapper via `IProductReadRepository`/`IOrderReadRepository`; Commands seguem 100% EF Core. Ver ADR-012.
- [ ] **Fase 7** — README final, revisão de `docker compose up`, checklist do enunciado.
