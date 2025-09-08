using System.ComponentModel.DataAnnotations;

namespace TPOR.Shared.Models;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime? ModifiedOn { get; set; }
    
    [MaxLength(100)]
    public string? ModifiedBy { get; set; }
}
