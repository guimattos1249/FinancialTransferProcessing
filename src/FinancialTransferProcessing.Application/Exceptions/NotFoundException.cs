namespace FinancialTransferProcessing.Application.Exceptions;

public class NotFoundException(string message) : ApplicationException(message)
{
}
