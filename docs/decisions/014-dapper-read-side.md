# ADR-014 — Dapper na leitura, EF Core dono da escrita

## Contexto

As queries de leitura são pass-through: buscam dado e devolvem, sem lógica de negócio no meio. É preciso decidir
se elas continuam passando pelo mesmo caminho das escritas ou se ganham um caminho próprio.

## Opções

- **Manter tudo em EF Core**, com o mesmo repositório por agregado servindo também às queries. Um único ORM e um
  único padrão de acesso a dado no projeto inteiro.
- **Repositório de leitura dedicado** por agregado. Separa leitura de escrita, mas cria uma interface com
  exatamente uma implementação possível — indireção sem outro consumidor previsto.
- **Dapper dentro do próprio handler de query**, sem repositório por agregado: cada slice concentra sua SQL, e o
  handler depende de um executor genérico que abstrai apenas o acesso à conexão.

## Decisão

As quatro queries de leitura usam Dapper através de um `IQueryExecutor` genérico, implementado na
Infrastructure. Os Commands seguem 100% em EF Core, sem mudança.

## Consequências

- Menos indireção para um caso de uso que não precisa dela: sem repositório-fachada de implementação única, sem
  lógica de negócio a proteger entre o banco e a resposta.
- Quebra conscientemente o isolamento que o resto do projeto mantém, onde só a Infrastructure sabe que o banco é
  PostgreSQL: a SQL de cada slice cita sintaxe do dialeto, e o pacote Dapper passa a ser referenciado também
  pela Application.
- A paginação melhora: a contagem total e a página vêm numa única ida ao banco, em vez dos dois round-trips que
  o caminho anterior fazia.
- Os métodos do Dapper são extension methods sobre a conexão e não são mockáveis diretamente. Por isso o handler
  nunca chama Dapper direto, só o executor — uma interface comum, fácil de mockar. Os testes de query seguem
  unitários, sem banco real, ao custo de não validarem a SQL em si, apenas a orquestração do handler.

## Referências

- [Dapper](https://github.com/DapperLib/Dapper)
- Vladimir Khorikov — [When does it make sense to use CQRS?](https://enterprisecraftsmanship.com/posts/cqrs-when-does-it-make-sense/)
