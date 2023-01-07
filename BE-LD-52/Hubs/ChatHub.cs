﻿using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BE_LD_52.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IUserService _userService;
        private readonly IGridService _gridService;
        private string _key;

        public ChatHub(IConfiguration config, IUserService userService, IGridService gridService)
        {
            _userService = userService;
            _gridService = gridService;
            _key = config.GetSection("InitKey").Value;
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

        public async Task InitializeGrid(int width, int height, string key)
        {
            if (_key != key)
                return;

            await _gridService.InitializeGrid(width, height);
        }

        public async Task GetGrid()
        {
            await Clients.All.SendAsync("ReceiveGrid");
        }

        public async Task GetCellInfo(int x, int y)
        {
            List<Cell> grid = new List<Cell>();

            var cell = await _gridService.GetCellInfo(x, y);
            await Clients.All.SendAsync("ReceiveCell", cell);
        }

        public async Task UpdateCell(Cell cellUpdate)
        {
            var cell = await _gridService.UpdateCell(cellUpdate);
            await Clients.All.SendAsync("ReceiveCell");
        }
    }
}
