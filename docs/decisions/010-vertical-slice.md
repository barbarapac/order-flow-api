# ADR-010 — Vertical Slice dentro de cada camada

## Contexto

O enunciado exige que a separação de responsabilidades exista, mas que a organização de pastas seja por Vertical
Slice, e não pela convenção tradicional de pastas técnicas dentro de um projeto único.

## Opções

- **Organização técnica dentro de cada camada** — `Commands/`, `Handlers/`, `Validators/`. Agrupa por tipo de
  arquivo: para entender uma feature inteira é preciso pular entre várias pastas.
- **Vertical Slice dentro de cada camada.** A pasta raiz é a feature, e tudo o que ela precisa fica junto.

## Decisão

Os quatro projetos continuam separados — esse é o limite real de compilação e de direção de dependência —, mas
dentro de cada um a pasta raiz é a feature. Pastas `_Shared/` guardam o que é genuinamente transversal.

## Consequências

- Mudar uma feature de ponta a ponta toca arquivos de uma pasta por camada, em vez de espalhar a mudança por
  várias pastas técnicas. Menor chance de esquecer uma peça.
- Em compensação, comparar duas features parecidas exige abrir pastas diferentes. É o trade-off clássico:
  otimiza para mudar uma feature, à custa de comparar várias do mesmo tipo lado a lado.

## Referências

- Jimmy Bogard — [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
