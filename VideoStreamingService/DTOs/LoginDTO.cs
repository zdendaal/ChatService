using System.ComponentModel.DataAnnotations;

namespace VideoStreamingService.DTOs
{
    /// <summary>
    /// Class serving only as DTO for user credentials
    /// </summary>
    public class LoginDTO
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
