using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ChatService.Database;
using ChatService.Models;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly BusinessData _businessData;

        public ChatsController(BusinessData businessData)
        {
            _businessData = businessData;
        }

        private long GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(claim, out var id) ? id : -1;
        }

        /// <summary>
        /// Get a list of quick detail of the chats of the user.
        /// </summary>
        /// <returns>List of chats with details.</returns>
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Token authorization failed.")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "List of chats with detail.")]
        public IActionResult List()
        {
            var userId = GetCurrentUserId();
            if (userId < 0) return Unauthorized();

            var chats = _businessData.Users
                .Where(x => x.Id == userId)
                .SelectMany(x => x.Chats)
                .Select(x => new
                {
                    Name = x.Chat.Name,
                    LastMsg = x.Chat.Messages.Select(x => new { Time = x.Timestamp, Text = x.Content }).LastOrDefault(),
                    Seen = x.LastSeen
                });

            return Ok(chats);
        }

        /// <summary>
        /// Creates a new chat with the specified name and assigns the current user as the owner.
        /// </summary>
        /// <remarks>The method requires the user to be authenticated. If the user is not authenticated,
        /// it returns an Unauthorized response. If the name is not provided, it returns a BadRequest response
        /// indicating that the name is required.</remarks>
        /// <param name="name">The name of the chat to be created. This parameter cannot be null or whitespace.</param>
        /// <returns>An IActionResult containing the ID and name of the newly created chat, or an appropriate error response.</returns>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Token authorization failed.")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Description = "Name is null or white space.")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Success. Returns chat Id and chat name of newly created chat.")]
        public IActionResult Create([FromQuery] string name)
        {
            var userId = GetCurrentUserId();
            if (userId < 0) return Unauthorized();
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

            var chat = new Chat { Name = name };
            _businessData.Chats.Add(chat);
            _businessData.SaveChanges();

            var member = new ChatMember { ChatId = chat.Id, UserId = userId, Role = ChatMemberRole.Owner };
            _businessData.ChatMembers.Add(member);
            _businessData.SaveChanges();

            return Ok(new { chat.Id, chat.Name });
        }

        /// <summary>
        /// Add member.
        /// </summary>
        /// <param name="chatId">Id of the chat.</param>
        /// <param name="userId">Id of the user to be added.</param>
        /// <returns></returns>
        [HttpPost("{chatId}/addMember")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Token authorization failed.")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Description = "If chat with id does not exists.")]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Description = "Caller adding himself.")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Description = "User to be added is already a member of the chat.")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Member was successfully added.")]
        public IActionResult AddMember(long chatId, [FromBody] long userId)
        {
            var caller = GetCurrentUserId();
            if (caller < 0) return Unauthorized();

            var chat = _businessData.Chats.Include(x => x.Members).SingleOrDefault(c => c.Id == chatId);
            if (chat == null) return NotFound();

            var callerIsOwner = chat.Members.Any(m => m.UserId == caller && (m.Role == ChatMemberRole.Owner || m.Role == ChatMemberRole.Maintainer));
            if (!callerIsOwner) return Forbid();

            if (chat.Members.Any(m => m.UserId == userId)) return BadRequest("User already a member");

            _businessData.ChatMembers.Add(new ChatMember { ChatId = chatId, UserId = userId, Role = ChatMemberRole.Member });
            _businessData.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Delete chat member.
        /// </summary>
        /// <param name="chatId">Id of the chat.</param>
        /// <param name="userId">Id of the user.</param>
        /// <returns></returns>
        [HttpDelete("{chatId}/deleteMember")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Token authorization failed.")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Description = "If chat with id does not exists or member with exact Id does not exist in the chat.")]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Description = "Caller has not enough permission.")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Member was successfully deleted.")]
        public IActionResult DeleteMember(long chatId, [FromQuery] long userId)
        {
            var caller = GetCurrentUserId();
            if (caller < 0) return Unauthorized();

            var chat = _businessData.Chats.Include(c => c.Members).SingleOrDefault(c => c.Id == chatId);
            if (chat == null) return NotFound();

            var callerIsOwner = chat.Members.Any(m => m.UserId == caller && (m.Role == ChatMemberRole.Owner || m.Role == ChatMemberRole.Maintainer));
            if (!callerIsOwner && caller != userId) return Forbid();

            var member = _businessData.ChatMembers.SingleOrDefault(m => m.ChatId == chatId && m.UserId == userId);
            if (member == null) return NotFound();

            _businessData.ChatMembers.Remove(member);
            _businessData.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Rename chat.
        /// </summary>
        /// <param name="chatId">Id of the chat.</param>
        /// <param name="name">New name.</param>
        /// <returns></returns>
        [HttpPost("{chatId}/rename")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Token authorization failed.")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Description = "If user has not enough permissions.")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Description = "If chat with id does not exists.")]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Description = "Caller has not enough permission.")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Chat was successfully renamed.")]
        public IActionResult Rename(long chatId, [FromQuery] string name)
        {
            var caller = GetCurrentUserId();
            if (caller < 0) return Unauthorized();
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required");

            var chat = _businessData.Chats.Include(c => c.Members).SingleOrDefault(c => c.Id == chatId);
            if (chat == null) return NotFound();

            var callerIsOwner = chat.Members.Any(m => m.UserId == caller && m.Role == ChatMemberRole.Owner);
            if (!callerIsOwner) return Forbid();

            chat.Name = name;
            _businessData.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Delete chat by Id.
        /// </summary>
        /// <param name="chatId">Id of the chat.</param>
        /// <returns>Http status code as result of an action.</returns>
        [HttpDelete("{chatId}/delete")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Token authorization failed.")]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Description = "If user has not enough permissions.")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Chat was successfully deleted.")]
        public IActionResult Delete(long chatId)
        {
            long userId = GetCurrentUserId();
            if (userId < 0) return Unauthorized();

            var caller = _businessData.ChatMembers.Include(x => x.Chat).SingleOrDefault(x => x.ChatId == chatId && x.UserId == userId && x.Role == ChatMemberRole.Owner);
            if(caller == null) return Forbid();

            _businessData.Chats.Remove(caller.Chat);
            _businessData.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// Change user role.
        /// </summary>
        /// <param name="chatId">Id of chat where caller has enough permission and target user.</param>
        /// <param name="userId">Id of the user.</param>
        /// <param name="role">Role of the user.</param>
        /// <returns></returns>
        [HttpPost("{chatId}/changeMemberRole")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Description = "Claims on token were not verified.")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Description = "Caller or target role user was not found.")]
        [ProducesResponseType(StatusCodes.Status403Forbidden, 
            Description = "Caller role is not high enough or in chat there would not be left other user with role high enough.")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Role successfully changed.")]
        public IActionResult ChangeRole(long chatId, long userId, ChatMemberRole role)
        {
            long callerId = GetCurrentUserId();
            if (callerId < 0) return Unauthorized();

            var caller = _businessData.ChatMembers.SingleOrDefault(x => x.ChatId == chatId && x.UserId == callerId);
            var user = _businessData.ChatMembers.SingleOrDefault(x => x.ChatId == chatId && x.UserId == userId);
            if (caller == null || user == null) return NotFound();
            if (user.Role == role) return Ok();
            if (caller.Role != ChatMemberRole.Owner) return Forbid();

            if(callerId == userId)
            {
                // there must be other member with Owner role in chat
                if (!_businessData.Chats.Where(x => x.Id == chatId).SelectMany(x => x.Members).Any(x => x.UserId != callerId && x.Role == ChatMemberRole.Owner))
                    return Forbid();
            }

            user.Role = role;
            _businessData.ChatMembers.Entry(user).State = EntityState.Modified;
            _businessData.SaveChanges();
            return Ok();
        }
    }
}
