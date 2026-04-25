using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class Company : BaseModel
{
    
    [StringLength(64)]
    public string? Name { get; set; }

}