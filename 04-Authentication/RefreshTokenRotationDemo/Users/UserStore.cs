namespace RefreshTokenRotationDemo.Users;

/// <summary>
/// Usuários fixos do exemplo. Autenticação de senha não é o assunto aqui — o foco é o
/// que acontece com os tokens depois que o login deu certo.
/// </summary>
public sealed class UserStore
{
    private static readonly IReadOnlyDictionary<string, DemoUser> Users = new Dictionary<string, DemoUser>(StringComparer.OrdinalIgnoreCase)
    {
        ["ana"] = new DemoUser("1", "ana", "senha123", "admin"),
        ["bruno"] = new DemoUser("2", "bruno", "senha123", "reader")
    };

    public DemoUser? Validate(string username, string password)
    {
        if (!Users.TryGetValue(username, out DemoUser? user))
        {
            return null;
        }

        return user.Password == password ? user : null;
    }

    public DemoUser? FindById(string userId)
    {
        foreach (DemoUser user in Users.Values)
        {
            if (user.Id == userId)
            {
                return user;
            }
        }

        return null;
    }
}

public sealed class DemoUser
{
    public DemoUser(string id, string username, string password, string role)
    {
        Id = id;
        Username = username;
        Password = password;
        Role = role;
    }

    public string Id { get; }

    public string Username { get; }

    public string Password { get; }

    public string Role { get; }
}
