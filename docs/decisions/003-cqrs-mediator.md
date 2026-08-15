# ADR-003 — CQRS com a lib Mediator, não MediatR

## Contexto

O projeto separa intenções de escrita das de leitura, cada uma com seu handler dedicado. É CQRS no nível da
aplicação, não da persistência: não há dois bancos nem dois modelos de dados, só handlers dedicados por
operação. Isso pede uma biblioteca de mediação em memória que roteie cada mensagem para o handler certo e
permita compor pipeline behaviors em torno deles.

## Opções

- **MediatR.** Padrão de facto no ecossistema .NET, amplamente conhecido, com roteamento e descoberta de
  handlers por reflection em runtime.
- **Mediator, de Martin Othamar.** Mesma proposta de API, mas o roteamento é gerado em tempo de compilação por
  source generator.

## Decisão

Usar o pacote `Mediator`, com Commands e Queries distinguidos por interfaces próprias.

## Consequências

- Sem reflection em runtime: o roteamento é código gerado e verificado na compilação, o que também torna a
  solução amigável a Native AOT e trimming.
- Em troca, o ecossistema é bem menor que o do MediatR, e o que lá é resolvido por assembly scan — o registro de
  pipeline behaviors — aqui precisa ser declarado manualmente.
- O nome do pacote é genérico o bastante para causar confusão com MediatR em revisão de código. Daí a nota
  explícita aqui e no `CLAUDE.md`.

## Referências

- [Mediator, de Martin Othamar](https://github.com/martinothamar/Mediator)
- [MediatR](https://github.com/jbogard/MediatR)
