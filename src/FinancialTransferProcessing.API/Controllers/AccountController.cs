using FinancialTransferProcessing.API.Filters;
using FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;
using FinancialTransferProcessing.Application.UseCases.Accounts.GetAccountById;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransferProcessing.API.Controllers;

public class AccountController : ApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
            [FromServices] ICreateAccountUseCase useCase,
            [FromBody] CreateAccountRequest request,
            CancellationToken cancellationToken)
    {
        var response = await useCase.Execute(request, cancellationToken);

        return Created(string.Empty, response);
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(GetAccountByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
            [FromServices] IGetAccountByIdUseCase useCase,
            [FromRoute] Guid Id,
            CancellationToken cancellationToken)
    {
        var response = await useCase.Execute(Id, cancellationToken);

        return Ok(response);
    }
}
