# Decisões técnicas

Uma decisão por arquivo, no estilo ADR: o contexto, as opções que estavam na mesa, o que foi escolhido e o que
isso custa. Aqui está o **porquê**; o **como** está em [`architecture.md`](../architecture.md),
[`domain-model.md`](../domain-model.md), [`concurrency.md`](../concurrency.md) e
[`error-handling.md`](../error-handling.md).

| # | Decisão |
|---|---|
| 001 | [IDs de agregado como Guid, gerados em memória](./001-ids-guid.md) |
| 002 | [Domínio em inglês, documentação em português](./002-idioma-dominio.md) |
| 003 | [CQRS com a lib Mediator, não MediatR](./003-cqrs-mediator.md) |
| 004 | [O cliente do pedido vem do JWT, sem entidade `Customer`](./004-sem-entidade-customer.md) |
| 005 | [Baixa de estoque só na confirmação](./005-baixa-estoque-na-confirmacao.md) |
| 006 | [Update condicional transacional para o estoque](./006-update-condicional-transacional.md) |
| 007 | [Lock distribuído via Redis, implementação manual](./007-lock-distribuido-redis.md) |
| 008 | [`Product` anêmico, `Order` rico](./008-product-anemico-order-rico.md) |
| 009 | [Guard Clauses no domínio, `Result<T>` na orquestração](./009-guard-clauses-domain-exceptions.md) |
| 010 | [Vertical Slice dentro de cada camada](./010-vertical-slice.md) |
| 011 | [JWT de 60 minutos, sem refresh token](./011-jwt-60-minutos.md) |
| 012 | [Migrations aplicadas no startup](./012-migrations-automaticas.md) |
| 013 | [Handler global de exception e tabela única de status](./013-iexceptionhandler-tabela-errortype.md) |
| 014 | [Dapper na leitura, EF Core dono da escrita](./014-dapper-read-side.md) |
| 015 | [CORS com origens explícitas vindas de configuração](./015-cors-origens-explicitas.md) |
| 016 | [Testes de arquitetura com ArchUnitNET](./016-testes-de-arquitetura.md) |

As três decisões que mais moldam o projeto são a 005, a 006 e a 007: juntas, elas respondem como o estoque nunca
fica negativo.
