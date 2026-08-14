namespace OrderFlow.Application.Users.Register;

public sealed record RegisterUserResponse(Guid Id, string Name, string Email, DateTime CreatedAtUtc);
