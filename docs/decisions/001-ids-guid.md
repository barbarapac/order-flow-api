# ADR-001 — IDs de agregado como Guid, gerados em memória

## Contexto

Toda raiz de agregado precisa de identificador único. O caminho mais comum em bancos relacionais é deixar o
banco gerar um sequencial, mas isso amarra a existência de um Id válido ao momento do insert: o objeto de
domínio não tem identidade própria até ser persistido.

## Opções

- **Sequencial gerado pelo banco.** Índice compacto e ordenação natural por criação. Em troca, expõe contagem de
  registros — quem vê `/orders/482` sabe que existem ao menos 482 pedidos e pode tentar os vizinhos, o clássico
  IDOR por enumeração — e obriga o objeto a existir sem identidade até o insert.
- **Guid gerado na criação do agregado.** O objeto nasce com identidade própria antes de qualquer contato com o
  banco. Não sequencial, não enumerável.

## Decisão

`User`, `Product` e `Order` usam `Guid`, gerado no próprio construtor ou factory do agregado.

## Consequências

- O agregado pode produzir um evento de domínio já com o Id definitivo, sem estado intermediário aguardando o
  banco.
- Não há enumeração sequencial de recursos por um cliente da API.
- Índice maior que um inteiro (16 bytes contra 4 ou 8) e sem localidade sequencial. Irrelevante neste volume,
  mas um ponto a revisitar em cenário de escrita muito intensa.

## Referências

- [OWASP — Insecure Direct Object Reference](https://cheatsheetseries.owasp.org/cheatsheets/Insecure_Direct_Object_Reference_Prevention_Cheat_Sheet.html)
