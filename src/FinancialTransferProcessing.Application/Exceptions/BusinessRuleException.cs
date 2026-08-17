namespace FinancialTransferProcessing.Application.Exceptions;

public class BusinessRuleException(string message)
    : ApplicationException(message);
