# Tarefa 6 — Implementar consulta de conta por ID

## Problema

Depois de criar uma conta, a API não oferece uma forma de consultar o recurso persistido. Os clientes não conseguem confirmar os dados e o saldo atual da conta utilizando seu identificador.

## Objetivo

Implementar um endpoint que consulte uma conta por ID e retorne seus dados atuais.

## Endpoint

```http
GET /api/v1/Account/{id}
```

## Resposta de sucesso

```http
200 OK
```

```json
{
  "id": "01900000-0000-0000-0000-000000000000",
  "name": "Conta principal",
  "balanceInCents": 10000,
  "version": 0,
  "createdAt": "2026-08-18T12:00:00Z"
}
```

## Conta inexistente

```http
404 Not Found
```

## Implementação esperada

- Criar `IGetAccountByIdUseCase`.
- Criar `GetAccountByIdUseCase`.
- Criar `GetAccountByIdResponse`.
- Consultar por meio de `IAccountReadOnlyRepository`.
- Lançar `NotFoundException` quando o repository retornar `null`.
- Registrar o use case na injeção de dependência da Application.
- Adicionar a action de consulta ao `AccountController`.
- Propagar `CancellationToken` do controller até o Entity Framework.
- Não retornar a entidade de domínio diretamente pelo controller.
- Utilizar `AsNoTracking` na consulta.

## Critérios de aceite

- Uma conta existente retorna HTTP 200.
- A resposta contém ID, nome, saldo, versão e data de criação.
- Uma conta inexistente retorna HTTP 404.
- Um identificador em formato inválido é rejeitado pela model binding.
- A entidade consultada não é rastreada pelo Entity Framework.
- O endpoint respeita cancelamentos da requisição.
- A solução compila sem erros ou avisos.
