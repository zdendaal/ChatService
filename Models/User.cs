using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace VideoStreamingService.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Nickname must be filled")]
        [StringLength(100, ErrorMessage = "Maximum " + nameof(Nickname) + "length is 100")]
        public string Nickname { get; set; }
        [EmailAddress(ErrorMessage = "Bad email format")]
        public string Email { get; set; }
        public string passwordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        [Required(ErrorMessage = "Country cannot be empty")]
        public string Country { get; set; }
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public IList<ChatMember> Chats { get; set; }
    }
}
