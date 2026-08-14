namespace OrderFlow.Domain._Shared;

public abstract class DomainException : Exception
{
    protected DomainException(string code, string message, ErrorType type) : base(message)
    {
        Code = code;
        Type = type;
    }

    public string Code { get; }

    public ErrorType Type { get; }
}
