using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoStreamingService.Models
{
    public class Chat
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Chat must have a name")]
        [StringLength(100, ErrorMessage = "Maximum chat name length is 100")]
        public string Name { get; set; }
        public IList<Message> Messages { get; set; }
        public IList<ChatMember> Members { get; set; }
    }
}
