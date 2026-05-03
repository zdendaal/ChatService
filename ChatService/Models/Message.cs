using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ChatService.Models
{
    public class Message
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Message cannot be empty")]
        [StringLength(BusinessSettings.messageNameMaxLength, MinimumLength = BusinessSettings.messageNameMinLength, ErrorMessage = "{0} length must be between {1} and {2} characters.")]
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(SenderId))]
        public long SenderId { get; set; }
        public User Sender { get; set; }
        [ForeignKey(nameof(ChatId))]
        public long ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}
