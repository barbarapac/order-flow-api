# ADR-002 — Domínio em inglês, documentação em português

## Contexto

O enunciado do desafio já define nomes em inglês para entidades e endpoints. É preciso decidir se o código segue
essa linguagem ubíqua ou traduz para português, e em qual idioma fica a documentação.

## Opções

- **Tudo em português.** Um idioma só no repositório, mas cria uma camada de tradução mental entre o enunciado e
  o código: quem avalia precisa mapear cada termo para conferir se a implementação bate com o pedido.
- **Tudo em inglês.** Elimina a mistura, mas perde naturalidade nas explicações mais longas.
- **Código em inglês, documentação em português.** O código cita o enunciado sem tradução; a explicação fica no
  idioma mais natural para quem a lê neste contexto.

## Decisão

Entidades, propriedades, comandos e endpoints seguem a linguagem ubíqua em inglês. A documentação é escrita em
português.

## Consequências

- Repositório com dois idiomas — aceito conscientemente, porque cada um está no lugar certo.
- Os códigos de erro acompanham o código: `{agregado}.{motivo}` em inglês, snake_case.

## Referências

- Eric Evans, *Domain-Driven Design* — capítulo sobre Ubiquitous Language.
