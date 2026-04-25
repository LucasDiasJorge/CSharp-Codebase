using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class User : BaseModel
{

    [StringLength(64)]
    public string Name { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
    
    public string Password { get; set; }

    public int CompanyId { get; set; }

    public virtual Company Company { get; set; }

}

