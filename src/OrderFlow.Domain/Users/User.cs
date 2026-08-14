namespace OrderFlow.Domain.Users;

public sealed class User
{
    public Guid Id                  { get; private set; }
    public string Name              { get; private set; }
    public ValueObjects.Email Email { get; private set; }
    public string PasswordHash      { get; private set; }
    public DateTime CreatedAtUtc    { get; private set; }

    private User(string name, ValueObjects.Email email, string passwordHash)
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

        return new User(name.Trim(), ValueObjects.Email.Create(emailRaw), passwordHash);
    }
}
