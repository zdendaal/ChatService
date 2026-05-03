using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Models
{
    /// <summary>
    /// Chat member data model -> connects users and chats, adds a role and joined date for each user in chat
    /// </summary>
    public class ChatMember
    {
        [ForeignKey(nameof(UserId))]
        public long UserId { get; set; }
        public User User { get; set; }
        [ForeignKey(nameof(ChatId))]
        public long ChatId { get; set; }
        public Chat Chat { get; set; }
        public ChatMemberRole Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// User roles in chat
    /// </summary>
    public enum ChatMemberRole
    {
        Owner,
        Maintainer,
        Member
    }
}
