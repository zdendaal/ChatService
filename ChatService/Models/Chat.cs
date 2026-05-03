using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.Models
{
    /// <summary>
    /// Chat data model
    /// </summary>
    public class Chat
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Chat must have a name")]
        [StringLength(BusinessSettings.chatNameMaxLength, MinimumLength = BusinessSettings.chatNameMinLength, ErrorMessage = "{0} name length must be between {1} and {2} characters.")]
        public string Name { get; set; } = string.Empty;
        public IList<Message> Messages { get; set; } = new List<Message>();
        public IList<ChatMember> Members { get; set; } = new List<ChatMember>();
    }
}
