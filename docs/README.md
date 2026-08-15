# Documentação — OrderFlow

O [README raiz](../README.md) mostra como rodar e usar a API. Estes documentos explicam **como** a solução é
construída e **por que** ela é assim.

| Documento | Responde |
|---|---|
| [`architecture.md`](./architecture.md) | Como as camadas se encaixam e por onde passa uma requisição |
| [`domain-model.md`](./domain-model.md) | Quais são as regras de negócio e quem as garante |
| [`concurrency.md`](./concurrency.md) | Como o estoque nunca fica negativo sob concorrência |
| [`error-handling.md`](./error-handling.md) | Como uma falha vira resposta HTTP |
| [`decisions/`](./decisions/) | Por que cada decisão foi tomada — 14 ADRs |

Se você tem cinco minutos, leia [`concurrency.md`](./concurrency.md): é o problema que orienta o resto do
projeto.
