using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Timers;

namespace BE_LD_52.Hubs
{
    public class ChatHub : Hub
    {
        private System.Timers.Timer _timer;
        private readonly IUserService _userService;
        private readonly IGridService _gridService;
        private readonly IHubContext<ChatHub> _hubContext;
        private string _key;
        private Queue<Cell> _cells = new Queue<Cell>();

        public ChatHub(IConfiguration config, IUserService userService, IGridService gridService, IHubContext<ChatHub> hubContext)
        {
            _userService = userService;
            _gridService = gridService;
            _hubContext = hubContext;
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
            var grid = await _gridService.GetGrid();
            await Clients.All.SendAsync("ReceiveGrid", grid);
        }

        public async Task GetCellInfo(int x, int y)
        {
            List<Cell> grid = new List<Cell>();

            var cell = await _gridService.GetCellInfo(x, y);
            await Clients.All.SendAsync("ReceiveCell", cell);
        }

        public async Task UpdateCell(string userId, int x, int y, string gameAction, string? cropType = null)
        {
            var cell = await _gridService.PrepareCell(userId, x, y, gameAction, cropType);

            if (cell != null)
            {
                _cells.Enqueue(cell);
                SetUpdateCellTimer(GetDuration(gameAction));
            }
        }

        private void SetUpdateCellTimer(double milliseconds)
        {
            // Create a timer with a two second interval.
            _timer = new System.Timers.Timer(milliseconds);
            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += OnCellUpdate;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private async void OnCellUpdate(Object source, ElapsedEventArgs e)
        {
            while(_cells.Count > 0)
            {
                var c = _cells.Peek();
                await _hubContext.Clients.All.SendAsync("ReceiveCell", c);
                _cells.Dequeue();
            }
        }

        private double GetDuration(string action)
        {
            var theAction = action.ToLower();
            switch (theAction)
            {
                case "till":
                        return 10000;
                case "sow":
                        return 10000;
                case "water":
                        return 10000;
                case "harvest":
                    return 10000;
                default: return 10000;
            }
        }
    }
}
