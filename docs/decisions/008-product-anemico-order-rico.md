# ADR-008 — `Product` anêmico, `Order` rico

## Contexto

O enunciado pede, ao mesmo tempo, um catálogo de produtos com domínio anêmico e um pedido com regras de negócio
reais — validação de itens, transições de estado, baixa de estoque. É preciso decidir onde fica o comportamento
de cada agregado.

## Opções

- **Os dois ricos**, com Guards e métodos de negócio também no produto. Mais consistente entre agregados, mas
  contraria o requisito explícito e adiciona complexidade onde ela não existe: um catálogo é, de fato, só
  cadastro neste domínio.
- **Os dois anêmicos**, deixando as regras do pedido na Application. Simplifica o agregado, mas espalha a
  máquina de estados por handlers e perde a garantia de que o objeto nunca existe em estado inválido — algo que
  só o próprio agregado consegue dar.
- **Produto anêmico, pedido rico**, cada um modelado conforme a complexidade que realmente carrega.

## Decisão

`Product` tem propriedades públicas e CRUD simples na Application, sem Guards nem eventos. `Order` e `OrderItem`
concentram invariantes e comportamento no próprio agregado, com Guard Clauses
([ADR-009](./009-guard-clauses-domain-exceptions.md)).

## Consequências

- Inconsistência deliberada entre os dois agregados. É proposital: o desafio testa justamente a capacidade de
  reconhecer quando um domínio precisa ser rico e quando complexidade adicional não traria valor.
- A baixa e a devolução de estoque não passam pelo produto em memória — são updates condicionais direto no
  repositório ([ADR-006](./006-update-condicional-transacional.md)), para não vazar regra de concorrência para
  uma entidade que deve ficar simples.

## Referências

- Martin Fowler — [AnemicDomainModel](https://martinfowler.com/bliki/AnemicDomainModel.html)
