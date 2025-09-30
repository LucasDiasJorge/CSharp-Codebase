using Microsoft.EntityFrameworkCore;

namespace Postgres.Models;

public class Users
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Roles Role { get; set; }
}