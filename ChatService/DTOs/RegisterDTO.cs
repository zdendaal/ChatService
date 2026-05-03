using System.ComponentModel.DataAnnotations;

namespace ChatService.DTOs
{
    /// <summary>
    /// Serving as DTO for user registration.
    /// </summary>
    public class RegisterDTO
    {
        [StringLength(BusinessSettings.nicknameMaxLength, MinimumLength = BusinessSettings.nicknameMinLength, ErrorMessage = "{0} length must be between {1} and {2}.")]
        public string Nickname { get; set;  }
        [Required]
        public string Email { get; set;  }
        [StringLength(BusinessSettings.passwordMaxLength, MinimumLength = BusinessSettings.passwordMinLength, ErrorMessage = "{0} length must be between {1} and {2}.")]
        public string Password { get; set;  }
        public string Country { get; set;  }
    }
}
