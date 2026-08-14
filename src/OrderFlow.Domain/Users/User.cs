namespace OrderFlow.Domain.Users;

public sealed class User
{
    public Guid Id               { get; private set; }
    public string Name           { get; private set; }
    public Email Email           { get; private set; }
    public string PasswordHash   { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private User(string name, Email email, string passwordHash)
    {
        Id           = Guid.NewGuid();
        Name         = name;
        Email        = email;
        PasswordHash = passwordHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    
    public static User Register(string name, string emailRaw, string passwordHash)
    {
        UserGuard.NameIsValid(name);
        UserGuard.PasswordHashIsValid(passwordHash);

        return new User(name.Trim(), Email.Create(emailRaw), passwordHash);
    }
}
