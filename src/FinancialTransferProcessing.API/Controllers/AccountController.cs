using FinancialTransferProcessing.API.Filters;
using FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransferProcessing.API.Controllers;

public class AccountController : ApiController
{
    [HttpPost("CreateAccount")]
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

}
