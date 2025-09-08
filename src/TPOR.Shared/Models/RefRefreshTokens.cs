using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPOR.Shared.Models;

[Table("RefRefreshTokens")]
public class RefRefreshToken : BaseEntity
{
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;
}
