using System.ComponentModel.DataAnnotations;

namespace SecurityAndAuthentication.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "User"; // Default role

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsSubscribed { get; set; } = false;
}

public class UserRegistrationDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3)]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool Subscribe { get; set; } = false;
}

public class UserLoginDto
{
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class TokenValidationDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class AssignRoleDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = string.Empty;
}
