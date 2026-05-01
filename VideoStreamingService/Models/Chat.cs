using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoStreamingService.Models
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
        public string Name { get; set; }
        public IList<Message> Messages { get; set; }
        public IList<ChatMember> Members { get; set; }
    }
}
