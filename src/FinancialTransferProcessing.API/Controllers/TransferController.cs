using FinancialTransferProcessing.API.Filters;
using FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;
using FinancialTransferProcessing.Application.UseCases.Transfers.GetTransferById;
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
            [FromHeader(Name = "X-Correlation-ID")] string? correlationId,
            [FromBody] CreateTransferRequest request,
            CancellationToken cancellationToken)
    {
        var response = await useCase.Execute(request, IdempotencyKey, correlationId, cancellationToken);

        return Accepted(string.Empty, response);
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(GetTransferByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(
            [FromServices] IGetTransferByIdUseCase useCase,
            [FromRoute] Guid Id,
            CancellationToken cancellationToken)
    {
        var response = await useCase.Execute(Id, cancellationToken);

        return Ok(response);
    }
}
