using FinancialTransferProcessing.API.Filters;
using FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FinancialTransferProcessing.API.Controllers;

public class TransferController : ApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateTransferResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
            [FromServices] ICreateTransferUseCase useCase,
            [FromHeader(Name = "Idempotency-Key"), BindRequired] Guid IdempotencyKey,
            [FromBody] CreateTransferRequest request,
            CancellationToken cancellationToken)
    {
        var response = await useCase.Execute(request, IdempotencyKey, cancellationToken);

        return Accepted(string.Empty, response);
    }
}
