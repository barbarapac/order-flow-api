namespace OrderFlow.Domain._Shared;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    BusinessRule,
    Unauthorized
}
