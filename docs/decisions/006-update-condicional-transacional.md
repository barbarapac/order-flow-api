# ADR-006 — Update condicional transacional para o estoque

## Contexto

Na confirmação ([ADR-005](./005-baixa-estoque-na-confirmacao.md)), vários pedidos podem tentar decrementar o
estoque do mesmo produto ao mesmo tempo. É preciso garantir que a quantidade disponível nunca fique negativa,
mesmo sob concorrência real no banco.

## Opções

- **Concorrência otimista por versão de linha.** O produto ganha uma coluna de versão e o update só afeta a
  linha se a versão não mudou; caso contrário o handler recarrega e tenta de novo. Funciona, mas exige coluna
  extra e laço de retry.
- **Update condicional sobre a própria regra de negócio.** Em vez de comparar versão, a cláusula `WHERE` embute
  a invariante real: a linha só é afetada se ainda houver estoque suficiente no momento exato do update.

## Decisão

A baixa acontece num único update que decrementa a quantidade disponível apenas se ela ainda for suficiente,
dentro da mesma transação que persiste o novo status do pedido. Nenhuma linha afetada significa conflito: uma
exception de estoque insuficiente é lançada e a transação inteira sofre rollback.

## Consequências

- Dispensa coluna extra e retry. A condição de negócio já é a checagem de concorrência, então não há falso
  positivo de conflito — uma versão de linha mudaria mesmo quando o saldo ainda fosse suficiente, forçando um
  retry desnecessário.
- O rollback é tudo ou nada por pedido. Um pedido de três itens onde só o terceiro falta estoque desfaz os três,
  evitando o cenário pior de "pedido meio confirmado". O custo é perder a baixa que já teria sido possível, mas
  o pedido fica em estado consistente para tentar de novo.
- Isso garante correção no banco, mas não impede várias réplicas da API de correrem em paralelo até chegar lá —
  ver [ADR-007](./007-lock-distribuido-redis.md).

## Referências

- [PostgreSQL — Concurrency Control](https://www.postgresql.org/docs/current/mvcc.html)
