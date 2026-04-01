using Microsoft.AspNetCore.SignalR;
using VideoStreamingService.Database;

namespace VideoStreamingService.Realtime
{
    public class Chat : Hub
    {
        private readonly BusinessData _businessData;

        public Chat(BusinessData businessData)
        {
            _businessData = businessData;
        }


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
                        chatId.ToString()?? throw new ArgumentNullException($"ChatId {chatId} in database is null, UserId {userId}"));
                });

            await base.OnConnectedAsync();
        }

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
                        chatId.ToString()?? throw new ArgumentNullException($"ChatId {chatId} in database is null, UserId {userId}")); 
                });

            await base.OnDisconnectedAsync(exception);
        }

        [HubMethodName("SendMessage")]
        public async Task SendMessage(string message, long chatId, long senderId)
        {
            var result = _businessData.Chats.Where(x => x.Id == chatId)
                .SelectMany(x => x.Members)
                .Any(x => x.UserId == senderId);

            if (!result) return;

            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", message, chatId, senderId);
        }
    }
}
