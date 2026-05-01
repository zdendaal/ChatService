using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;


namespace VideoStreamingService.Models
{
    /// <summary>
    /// User data model
    /// </summary>
    public class User
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Nickname must be filled.")]
        [StringLength(BusinessSettings.nicknameMaxLength, ErrorMessage = "{0} length must be between {1} and {2} characters.")]
        public required string Nickname { get; set; }
        [EmailAddress(ErrorMessage = "Bad email format")]
        public required string Email { get; set; }
        public required string passwordHash { get; set; } = string.Empty;
        public required DateTime CreatedAt { get; set; }
        [Required(ErrorMessage = "Country cannot be empty.")]
        public required string Country { get; set; }
        public required string ProfilePictureUrl { get; set; } = string.Empty;
        public IList<ChatMember> Chats { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
    }

    public enum UserRole
    {
        User,
        Moderator,
        Admin
    }
}
