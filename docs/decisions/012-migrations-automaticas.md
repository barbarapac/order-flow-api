# ADR-012 — Migrations aplicadas no startup

## Contexto

O enunciado pede que um `docker compose up` suba API e banco prontos para uso, com o schema criado, sem passo
manual adicional para quem avalia.

## Opções

- **Migration como passo de deploy separado**, rodada fora do processo da API. É a prática mais segura para
  produção: separa o ciclo de vida do schema do ciclo de vida da aplicação e permite revisar a migration antes
  de aplicá-la.
- **Migration aplicada no boot da própria API.** Nenhum passo manual — a aplicação cuida do próprio schema ao
  subir.

## Decisão

A API aplica as migrations pendentes no boot, atrás de uma flag de configuração habilitada por padrão em Docker
e desenvolvimento.

## Consequências

- Atende exatamente o requisito: `docker compose up` sobe tudo pronto.
- Não é a abordagem recomendada em produção real, onde várias réplicas subindo juntas poderiam tentar migrar ao
  mesmo tempo, ou onde se deseja revisar o SQL antes de aplicá-lo. Registrado aqui como trade-off consciente
  para o escopo do desafio, não como recomendação geral.

## Referências

- [EF Core — Applying migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
