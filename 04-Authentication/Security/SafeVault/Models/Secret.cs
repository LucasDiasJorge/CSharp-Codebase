using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models;

public class Secret
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(5000)]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public User? User { get; set; }
}
