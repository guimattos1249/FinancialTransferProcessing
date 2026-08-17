namespace FinancialTransferProcessing.API.Filters;

public class ResponseError
{
    public IList<string> Errors { get; set; }

    public bool TokenIsExpired { get; set; }

    public ResponseError(IList<string> errors) => Errors = errors;

    public ResponseError(string error)
    {
        Errors = new List<string>
            {
                error
            };
    }
}
