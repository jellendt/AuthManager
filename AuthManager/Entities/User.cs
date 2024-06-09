using AuthManager.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthManager.Entities
{
    [Table("Users")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Salt { get; set; }
        public required string EMail { get; set; }
        public RoleEnum Role { get; set; }
        [NotMapped]
        public string? JwtToken { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = [];
        public RefreshToken? ActiveRefreshToken
        {
            get
            {
                return this.RefreshTokens.FirstOrDefault(rf => !rf.IsExpired);
            }
        } 
    }
}
