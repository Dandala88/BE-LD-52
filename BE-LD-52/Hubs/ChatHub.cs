using BE_LD_52.Models;
using Microsoft.AspNetCore.SignalR;

namespace BE_LD_52.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(GameAction action)
        {
            System.Diagnostics.Debug.WriteLine(action.UserId);
            await Clients.All.SendAsync("ReceiveMessage", action);
        }
    }
}
