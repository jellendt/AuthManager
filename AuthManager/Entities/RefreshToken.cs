using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthManager.Entities
{
    [Owned]
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [JsonIgnore]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
        public Guid ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }
        public bool IsExpired 
        {
            get
            {
                return DateTime.UtcNow >= this.Expires;
            }
        }
        public bool IsRevoked
        {
            get
            {
                return this.Revoked != null;
            }
        }
        public bool IsActive
        {
            get
            {
                return !this.IsRevoked && !this.IsExpired;
            }
        }
    }
}
