# ADR-009 — Guard Clauses no domínio, `Result<T>` na orquestração

## Contexto

Toda invariante de agregado — construção válida, transição de estado permitida — precisa de um jeito consistente
de ser verificada e sinalizada. A escolha é entre lançar exception ou devolver um tipo que representa
sucesso ou falha explicitamente na assinatura.

## Opções

- **`Result<T>` para tudo**, inclusive construção e transição. Mantém o fluxo de erro explícito no tipo de
  retorno, mas não funciona bem em construtores: um construtor ou constrói um objeto válido ou não constrói
  nada, e não há meio-termo que uma assinatura de resultado consiga expressar sem transformar toda construção
  num factory method.
- **Exception para tudo**, inclusive "não encontrado". Simples de escrever, mas usa uma ferramenta pensada para
  o excepcional num caminho esperado e frequente: buscar um Id que não existe não é falha do sistema, é uma
  resposta válida.
- **Cada mecanismo onde encaixa melhor.**

## Decisão

Invariantes de agregado são validadas por Guard Clauses estáticas, chamadas no início de factories e métodos de
transição, que lançam a exception selada daquele agregado assim que a invariante é violada — sem deixar o objeto
existir em estado inválido. `Result<T>` cobre só os outcomes que o agregado não tem como avaliar sozinho, hoje
essencialmente "não encontrei esse recurso no repositório".

A régua é simples: se o agregado decide sozinho, é exception; se depende de consultar outra coisa, é
`Result<T>`.

## Consequências

- Só `User` e `Order` usam Guard. `Product`, anêmico por decisão ([ADR-008](./008-product-anemico-order-rico.md)),
  é validado 100% por FluentValidation na Application.
- Duas formas de sinalizar erro convivem no mesmo código, o que exige disciplina para não misturar. Como as duas
  vias convergem para o mesmo formato de resposta HTTP está em
  [ADR-013](./013-iexceptionhandler-tabela-errortype.md).
