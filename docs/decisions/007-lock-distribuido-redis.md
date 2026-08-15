# ADR-007 — Lock distribuído via Redis, implementação manual

## Contexto

O update condicional ([ADR-006](./006-update-condicional-transacional.md)) já garante que o estoque não fica
negativo, mas com várias réplicas da API todas competem pelo mesmo produto, cada uma tentando o update por conta
própria. É desejável serializar essa região crítica antes de tocar o banco.

## Opções

- **Bibliotecas prontas de lock distribuído** como RedLock.net ou Medallion.Threading. Implementam o algoritmo
  Redlock, pensado para coordenar múltiplas instâncias independentes de Redis por quorum. Este projeto roda com
  uma instância só — a complexidade do algoritmo multi-instância não se aplica.
- **Implementação manual sobre o cliente Redis**, usando os primitivos atômicos que o próprio Redis oferece.

## Decisão

O lock é adquirido por uma gravação condicional com expiração, que só grava se a chave não existir, e a espera é
por polling curto enquanto a chave estiver ocupada. A liberação roda como script atômico que só apaga a chave se
o valor guardado ainda for o token daquela aquisição.

São dois locks: um por produto, com os Ids ordenados antes da aquisição para evitar deadlock em pedidos
multi-item; e um por pedido, adquirido logo no início da confirmação e do cancelamento, para proteger a checagem
de idempotência contra as duas operações rodando em paralelo. O funcionamento está detalhado em
[`concurrency.md`](../concurrency.md).

## Consequências

- Controle total do comportamento, sem trazer uma dependência — e sua configuração — para resolver um problema
  de coordenação entre instâncias de Redis que este projeto não tem.
- As interfaces do cliente Redis são mockáveis, então os testes de lock rodam sem um Redis real.
- Se um dia for preciso rodar contra um cluster Redis de verdade, esta decisão precisa ser revisitada: o que
  está implementado aqui não é Redlock e não garante correção nesse cenário.
- A espera por polling é mais simples de implementar e testar que um mecanismo orientado a evento, ao custo de
  latência extra sob alta contenção — a liberação nunca é percebida instantaneamente.

## Referências

- [Redis — Distributed Locks](https://redis.io/docs/latest/develop/use/patterns/distributed-locks/)
- [RedLock.net](https://github.com/samcook/RedLock.net) — considerado, não usado
