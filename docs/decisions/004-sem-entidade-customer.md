# ADR-004 — O cliente do pedido vem do JWT, sem entidade `Customer`

## Contexto

Todo pedido precisa saber quem o fez. O enunciado sugere, no payload mínimo da criação de pedido, um campo
`customerId` — o que apontaria para uma entidade `Customer` distinta do `User` que se autentica na API.

## Opções

- **Criar `Customer` separado de `User`**, com cadastro próprio, recebendo o Id no payload do pedido.
- **Usar o próprio usuário autenticado como cliente**, derivando o Id do claim do JWT e não aceitando esse campo
  no payload.

## Decisão

Não existe entidade `Customer`. O cliente do pedido é sempre o usuário autenticado, extraído do token, nunca
recebido no corpo da requisição.

## Consequências

- Desvia do payload mínimo sugerido no enunciado. Desvio consciente, por isso documentado.
- Ganha em segurança: ninguém cria pedido em nome de outro só por saber o Id dele. Se o campo viesse no payload,
  toda rota que o recebesse precisaria de uma checagem extra contra o token — e esquecer essa checagem num
  endpoint novo ficaria sempre à espreita.
- Evita uma entidade sem função real: uma tabela a mais só para espelhar `User` seria complexidade sem
  benefício no escopo do desafio.
