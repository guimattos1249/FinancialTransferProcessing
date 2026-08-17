using FinancialTransferProcessing.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinancialTransferProcessing.API.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = context.Exception switch
        {
            ErrorOnValidationException exception =>
                new BadRequestObjectResult(
                    new ResponseError(exception.ErrorMessages)),

            NotFoundException exception =>
                new NotFoundObjectResult(
                    new ResponseError(exception.Message)),

            BusinessRuleException exception =>
                new UnprocessableEntityObjectResult(
                    new ResponseError(exception.Message)),

            _ => new ObjectResult(new ResponseError("Unknown error."))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };

        context.ExceptionHandled = true;
    }
}
