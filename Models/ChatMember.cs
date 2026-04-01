namespace VideoStreamingService.Models
{
    public class ChatMember
    {
        public long UserId { get; set; }
        public User User { get; set; }
        public long ChatId { get; set; }
        public Chat Chat { get; set; }
        public ChatMemberRole Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ChatMemberRole
    {
        Owner,
        Member
    }
}
