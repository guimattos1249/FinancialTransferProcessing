# ADR 0001 — Usar RabbitMQ.Client para a infraestrutura de mensageria

**Status:** Aceita
**Data:** 2026-08-27
**Escopo:** Tasks 12 a 18 — processamento assíncrono

## Contexto

O projeto precisa declarar uma topologia RabbitMQ explícita, publicar o outbox com publisher confirms, consumir mensagens com acknowledgements manuais e implementar retry, dead letter, recuperação de conexão e encerramento gracioso.

As Tasks 10 e 11 já introduziram um transactional outbox, um contrato versionado e uma política de serialização próprios. As tarefas seguintes foram planejadas para tornar visíveis e verificáveis as garantias de entrega, idempotência e recuperação oferecidas pela combinação entre aplicação e broker.

Foram consideradas três alternativas:

1. Usar MassTransit v9, que fornece abstrações maduras para topologia, consumers, publisher confirms, retry, redelivery, outbox e observabilidade, mas possui licenciamento comercial para uso em produção.
2. Usar MassTransit v8, que permanece disponível sob sua licença open source original, mas representa uma linha legada e não está coberta pelo acordo de suporte e manutenção do v9.
3. Usar o cliente oficial `RabbitMQ.Client`, mantendo no projeto a responsabilidade pelos mecanismos de confiabilidade necessários.

Referências consultadas na data desta decisão:

- [Licenciamento do MassTransit](https://massient.com/license)
- [Configuração RabbitMQ do MassTransit](https://masstransit.io/documentation/configuration/transports/rabbitmq)
- [Guia oficial do RabbitMQ.Client para .NET](https://www.rabbitmq.com/client-libraries/dotnet-api-guide)
- [Guia de confiabilidade do RabbitMQ](https://www.rabbitmq.com/docs/reliability)

## Decisão

Usaremos `RabbitMQ.Client`, o cliente oficial AMQP 0-9-1 para .NET, como dependência da camada `Infrastructure`.

A integração deverá:

- Permanecer encapsulada em `FinancialTransferProcessing.Infrastructure`.
- Não expor tipos de `RabbitMQ.Client` para `Application` ou `Domain`.
- Centralizar opções, nomes de exchanges, filas, routing keys e argumentos da topologia.
- Usar conexões e canais de longa duração, respeitando as regras de concorrência do cliente.
- Declarar a topologia de forma durável e idempotente.
- Publicar mensagens persistentes com publisher confirms.
- Consumir com acknowledgement manual somente após o processamento bem-sucedido.
- Tratar redelivery como uma possibilidade normal e exigir processamento idempotente.
- Implementar retry, dead letter, recuperação e graceful shutdown nas tarefas específicas do backlog.
- Evitar a criação de um framework de mensageria genérico; a implementação deve atender somente aos casos de uso do projeto.

Essa escolha é deliberadamente contextual. Ela busca controle explícito e aprendizado dos mecanismos de confiabilidade, e não afirma que clientes de baixo nível sejam sempre preferíveis a um service bus.

## Consequências positivas

- As garantias de publicação e consumo ficam explícitas e demonstráveis.
- A topologia permanece exatamente alinhada aos nomes definidos pelo backlog.
- O outbox e o envelope já existentes continuam sendo utilizados sem um segundo modelo de persistência.
- O projeto não depende do licenciamento de um framework de service bus.
- O conteúdo técnico pode explicar os mecanismos fundamentais em vez de apenas a configuração de uma abstração de alto nível.

## Consequências negativas

- O projeto assume responsabilidade por lifecycle de conexões e canais, confirmações, acknowledgements, concorrência, recuperação e tratamento de falhas.
- Há mais código de infraestrutura para implementar, testar, observar e manter.
- Uma implementação incorreta pode provocar perda, duplicação, loops de redelivery ou esgotamento de recursos.
- Recursos oferecidos por frameworks maduros, como sagas e múltiplos transports, não estarão disponíveis automaticamente.

## Critérios para reconsideração

Esta decisão deverá ser revisitada se:

- O número de mensagens, consumers e políticas crescer a ponto de formar um framework interno.
- O sistema passar a precisar de sagas, routing slips, request/response ou múltiplos brokers.
- A manutenção da camada própria demonstrar custo ou risco maior que o licenciamento de uma solução madura.
- Surgir uma exigência de suporte comercial para a infraestrutura de mensageria.
- Testes operacionais indicarem que a implementação própria não atende às garantias de confiabilidade do sistema.

