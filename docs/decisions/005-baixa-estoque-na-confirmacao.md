# ADR-005 — Baixa de estoque só na confirmação

## Contexto

Um pedido passa por dois momentos que poderiam mexer no estoque: a criação e a confirmação. É preciso decidir em
qual deles o estoque é efetivamente decrementado.

## Opções

- **Reservar já na criação.** Garante que todo pedido criado tem estoque, mas exige um mecanismo de expiração e
  liberação para pedidos que nunca são confirmados — senão o estoque fica preso atrás de carrinhos abandonados.
- **Só validar na criação e decrementar na confirmação.** A criação responde rápido e não compromete estoque; a
  baixa real acontece quando o pedido é de fato confirmado.

## Decisão

A criação apenas verifica se há quantidade suficiente para cada item, sem decrementar nada. A confirmação é o
único ponto que baixa estoque. O cancelamento só devolve se o pedido já estava confirmado.

## Consequências

- Toda a lógica de concorrência crítica fica concentrada em um único ponto de entrada, o que simplifica onde
  lock e transação precisam existir.
- Consequência aceita: um pedido pode ser criado e, na hora de confirmar, não haver mais estoque, porque outro
  pedido foi confirmado antes. A confirmação responde `409` e o pedido permanece `Placed` — cabe ao cliente
  cancelar ou tentar de novo. Preferimos esse risco raro, sinalizado com erro claro, a manter estoque reservado
  indefinidamente.
