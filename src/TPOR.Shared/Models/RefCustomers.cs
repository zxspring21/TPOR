using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("RefCustomers")]
public class RefCustomer : BaseEntity
{
    [MaxLength(50)]
    public string CustomerCode { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
