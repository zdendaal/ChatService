using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ChatService.Database;
using ChatService.Models;
using ChatService.Services;
using ChatService.DTOs;

namespace ChatService.APIs
{
    [ApiController]
    [Route("[controller]")]
    public class Gateway : ControllerBase    
    {
        public readonly BusinessData _businessData;
        public readonly Token _token;

        public Gateway([FromServices] BusinessData businessData, [FromServices] Token token)
        {
            _businessData = businessData;
            _token = token;
        }

        /// <summary>
        /// Register user account with email, password and country. Email must be unique, password is hashed before saving to database.
        /// </summary>
        /// <param name="details">Register DTO with credentials.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO details)
        {
            if (details == null) return BadRequest("Missing registration data.");

            if (_businessData.Users.Any(x => x.Email == details.Email))
                return BadRequest("Email already exists");
            if (_businessData.Users.Any(x => x.Nickname == details.Nickname))
                return BadRequest("Nickname already exists.");

            var hasher = new PasswordHasher<object>();
            string hashed = hasher.HashPassword(details.Email, details.Password);
            var user = new User
            {
                Email = details.Email,
                passwordHash = hashed,
                Country = details.Country,
                Nickname = details.Nickname,
                ProfilePictureUrl = "https://www.gravatar.com/avatar/" + Guid.NewGuid().ToString("N") + "?d=identicon",
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            _businessData.Users.Add(user);
            await _businessData.SaveChangesAsync();
            return Ok("User registered successfully");
        }

        /// <summary>
        /// Login to user account.
        /// </summary>
        /// <param name="creds">Login DTO with credentials.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO creds)
        {
            var hasher = new PasswordHasher<object>();
            string hashed = hasher.HashPassword(creds.Email, creds.Password);

            var result = _businessData.Users.Where(x => x.Email == creds.Email)
                .Select(x => new { hash = x.passwordHash, id = x.Id, role = x.Role } )
                .SingleOrDefault();

            if (result == null || hashed != result.hash) return Unauthorized("Bad email or password.");

            string token = _token.GenerateJwtToken(creds.Email, result.id.ToString(), result.role);
            return Ok(token);
        }
    }
}
