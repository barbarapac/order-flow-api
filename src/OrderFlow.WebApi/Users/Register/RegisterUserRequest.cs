namespace OrderFlow.WebApi.Users.Register;

public sealed record RegisterUserRequest(string Name, string Email, string Password);
