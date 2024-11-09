using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthManager.Entities
{
    public class FidoCredential
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte[] Id { get; set; }
        public Guid UserId { get; set; }
        public byte[] PublicKey { get; set; }
    }
}
