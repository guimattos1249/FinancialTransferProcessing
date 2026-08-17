using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialTransferProcessing.Application.Exceptions;

public class ErrorOnValidationException : ApplicationException
{
    public IList<string> ErrorMessages { get; set; }

    public ErrorOnValidationException(IList<string> errorMessage) : base(string.Empty) => ErrorMessages = errorMessage;
}
