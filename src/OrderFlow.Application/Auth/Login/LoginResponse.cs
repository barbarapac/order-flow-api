namespace OrderFlow.Application.Auth.Login;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Name,
    string Email);
