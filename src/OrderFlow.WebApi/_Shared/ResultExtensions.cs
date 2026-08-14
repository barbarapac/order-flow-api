using OrderFlow.Domain._Shared;

namespace OrderFlow.WebApi._Shared;

public static class ResultExtensions
{
    public static IResult ToProblemResult(this Error error) =>
        Results.Problem(
            statusCode: error.Type.ToStatusCode(),
            title: error.Message,
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code });
}
