# ADR-016 — Testes de arquitetura com ArchUnitNET

## Contexto

As regras estruturais do projeto — direção das dependências entre camadas, independência dos slices, nome e
visibilidade de cada papel — estavam escritas em [`architecture.md`](../architecture.md), nos ADRs, mas nada as executava.

Parte delas o compilador já garante de graça, pelo grafo de `ProjectReference`: `Domain` não enxerga
`Application`, `Application` não enxerga `Infrastructure`. O que sobra sem rede de proteção é justamente o que
mais dói quando escapa:

- a `WebApi` referencia `Infrastructure` e `Domain` (precisa, para compor o DI e aplicar migrations no boot),
  então nada impede um endpoint de injetar `IOrderRepository` ou o `OrderFlowDbContext` e pular a `Application`;
- a pureza de `Domain` e `Application` depende só do `.csproj` — um `PackageReference` novo derruba a barreira
  em silêncio, sem nenhum erro de compilação;
- a independência entre slices ([ADR-010](./010-vertical-slice.md)) não tem barreira nenhuma: um `using` entre
  pastas de feature compila normalmente;
- convenções de nome, `sealed`, `internal` e `static` dependiam de revisão de PR.

## Opções

- **Manter só a documentação e a revisão de PR.** Custo zero, eficácia proporcional à atenção de quem revisa —
  e erosão arquitetural é exatamente o tipo de coisa que passa em revisão porque cada violação isolada parece
  inofensiva.
- **Analisadores Roslyn próprios.** Poder total, feedback na IDE enquanto se digita. Custo de escrita e
  manutenção alto demais para o tamanho do projeto.
- **[NetArchTest.Rules](https://github.com/BenMorris/NetArchTest).** API fluente enxuta, resolve as regras de
  camada e de nome. Não tem regra de *slice*, que aqui é a mais valiosa, e o repositório está praticamente
  parado desde 2021.
- **[ArchUnitNET](https://github.com/TNG/ArchUnitNET).** Porte do ArchUnit do Java, mantido ativamente pela
  TNG. Cobre camadas, nome, visibilidade, herança e atributos.

## Decisão

ArchUnitNET, em um projeto de teste próprio: `test/OrderFlow.ArchitectureTest`.

O projeto é separado do `OrderFlow.UnitTest` de propósito. O teste de arquitetura precisa carregar os **quatro**
assemblies, `Infrastructure` e `WebApi` inclusive; o `UnitTest` referencia apenas `Domain` e `Application`, e
essa restrição é ela própria uma regra que vale a pena preservar.

Os assemblies são carregados uma vez em `Fixtures/ArchitectureFixture.cs`, de onde saem os quatro seletores de
camada. As regras ficam em quatro arquivos, por natureza:

| Arquivo | Garante |
|---|---|
| `LayerDependencyTests` | Direção das dependências e pureza de `Domain`/`Application` quanto a EF Core, ASP.NET, Npgsql e Redis; endpoint sem `Infrastructure` nem repositório |
| `VerticalSliceTests` | Nenhum slice depende de um slice vizinho, na `Application` e na `WebApi` |
| `ConventionTests` | Nome, `sealed`, `internal`/`static` e contrato de Commands, Queries, Handlers, Validators, Endpoints, Guards e exceptions |
| `MediatorContractTests` | Todo Command/Query tem exatamente um handler |

Três detalhes da implementação que não são óbvios:

- **O assembly da `WebApi` é alcançado por `typeof(IEndpoint)`**, não por `typeof(Program)`: com top-level
  statements a classe `Program` é `internal`.
- **Os slices são descobertos por reflection** (os namespaces de dois níveis abaixo da raiz da camada,
  descartando `_Shared`) e viram um caso de `[Theory]` cada. Criar uma feature nova não exige tocar no teste —
  ela entra na regra sozinha, e a mensagem de falha aponta o slice exato.
- **A regra de slice não usa a API `Slices()` da lib.** Ela trata `Application._Shared` como mais um slice e
  acusa como violação toda dependência legítima ao `_Shared`. A comparação é feita por padrão de namespace, com
  o `_Shared` deliberadamente fora do conjunto.

O `ValidationBehavior` não entra numa regra "todo Command tem um Validator": `Confirm`, `Cancel`, `Delete` e os
`GetById` só recebem `Guid` de rota e do claim, e não têm payload a validar. A regra existiria só para ser
suprimida.

No CI, os testes de arquitetura rodam em passo próprio, sem cobertura — o gate de cobertura continua medindo
apenas `[OrderFlow.Domain]` e `[OrderFlow.Application]`.

## Consequências

- Violar uma regra estrutural quebra o build com a mensagem apontando o tipo e a dependência exata
  (`ConfirmOrderEndpoint does depend on "OrderRepository"`), não em uma discussão de revisão de PR.
- A documentação passa a ter uma verificação: se um ADR mudar e o teste não, o teste denuncia a divergência.
- ArchUnitNET exige **avaliação positiva** por padrão — uma regra cujo seletor não casa com nenhum tipo falha
  com "The rule requires positive evaluation", em vez de passar vazia. Isso elimina a falha mais comum desse
  tipo de teste, que é a regra que só parece estar protegendo alguma coisa.
- Toda regra é uma decisão que passa a ter custo de manutenção. Foram escritas apenas regras que correspondem a
  decisões já registradas em ADR; exceções conscientes ficam explícitas no próprio teste — o Dapper permitido na
  `Application` ([ADR-014](./014-dapper-read-side.md)) e a `InsufficientStockException` como única exception
  nascida ali ([ADR-006](./006-update-condicional-transacional.md)).
- A análise é sobre metadados do assembly: dependência que só existe em string (o SQL do Dapper, um nome de
  tipo em configuração) continua invisível para essas regras.
- Custo de execução irrelevante: a suíte inteira roda em menos de meio segundo, e a carga dos assemblies leva
  cerca de um segundo.

## Referências

- [ArchUnitNET](https://github.com/TNG/ArchUnitNET)
- [ADR-010 — Vertical Slice dentro de cada camada](./010-vertical-slice.md)
