using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ChatService.Database;

namespace ChatService.Realtime
{
    /// <summary>
    /// Class defining SingalR methods for chatting
    /// </summary>
    [Authorize]
    public class Chat : Hub
    {
        private readonly BusinessData _businessData;

        public Chat(BusinessData businessData)
        {
            _businessData = businessData;
        }

        /// <summary>
        /// Invoked when a new client connection is established. Adds the connected user to all chat groups associated
        /// with their user ID.
        /// </summary>
        /// <remarks>This method retrieves the user ID from the connection context and, if valid, adds the
        /// user to each chat group they are a member of. If the user ID is not valid or not present, the method
        /// completes without adding the user to any groups.</remarks>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if a chat ID retrieved from the database is null while adding the user to a group.</exception>
        public override async Task OnConnectedAsync()
        {
            long userId = long.TryParse(Context.UserIdentifier, out var id) ? id : -1;
            if (userId < 0)
                return;

            _businessData.Users.Where(x => x.Id == userId)
                .Select(x => x.Chats.Select(x => x.ChatId))
                .ToList()
                .ForEach(chatId =>
                {
                    Groups.AddToGroupAsync(
                        Context.ConnectionId,
                        chatId.ToString()?? throw new ArgumentNullException($"ChatId {chatId} in database is null, UserId {userId}."));
                });

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Invoked when a existing client connection is terminated. Removes the disconnected user from all chat groups associated.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throws if a chat ID retrieved from database is null while adding the user to group.</exception>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            long userId = long.TryParse(Context.UserIdentifier, out var id) ? id : -1;
            if (userId < 0)
                return;

            _businessData.Users.Where(x => x.Id == userId)
                .Select(x => x.Chats.Select(x => x.ChatId))
                .ToList()
                .ForEach(chatId => 
                { 
                    Groups.RemoveFromGroupAsync(
                        Context.ConnectionId, 
                        chatId.ToString()?? throw new ArgumentNullException($"ChatId {chatId} in database is null, UserId {userId}.")); 
                });

            await base.OnDisconnectedAsync(exception);
        }


        /// <summary>
        /// Invoked when some client sends message
        /// </summary>
        /// <param name="message">Message text content.</param>
        /// <param name="chatId">ID of chat where message belongs.</param>
        /// <param name="senderId">ID of user that sent the message.</param>
        /// <returns></returns>
        [HubMethodName("SendMessage")]
        public async Task SendMessage(string message, long chatId, long senderId)
        {
            var result = _businessData.Chats.Where(x => x.Id == chatId)
                .SelectMany(x => x.Members)
                .Any(x => x.UserId == senderId && x.ChatId == chatId);

            if (!result) return;

            var newMessage = new Models.Message
            {
                Content = message,
                ChatId = chatId,
                SenderId = senderId
            };
            _businessData.Messages.Add(newMessage);
            _businessData.SaveChanges();

            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", message, chatId, senderId);
        }
    }
}
