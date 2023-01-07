using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BE_LD_52.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IUserService _userService;

        public ChatHub(IUserService userService)
        {
            _userService = userService;
        }

        public async Task GetUser(GameUser gameUser)
        {
            var userData = await _userService.GetUserData(gameUser);
            await Clients.Caller.SendAsync("ReceiveUser", userData);
        }

        public async Task SendMessage(GameAction action)
        {
            await Clients.All.SendAsync("ReceiveMessage", action);
        }
    }
}
