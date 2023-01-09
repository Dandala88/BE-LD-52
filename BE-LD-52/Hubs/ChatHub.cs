using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Timers;

namespace BE_LD_52.Hubs
{
    public class ChatHub : Hub
    {
        private double _durationMs = 10000;
        private System.Timers.Timer _timer;
        private readonly IUserService _userService;
        private readonly IGridService _gridService;
        private readonly IHubContext<ChatHub> _hubContext;
        private string _key;
        private double _configMs;
        private Queue<Cell> _cells = new Queue<Cell>();
        private Queue<GameUser> _gameUsersCollectingWater = new Queue<GameUser>();
        private List<GameUser> _gameUsers = new List<GameUser>();

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
            var user = await _userService.GetUserData(new GameUser() { id = userId });
            user.ConnectionId = Context.ConnectionId;
            await _userService.UpdateUser(user);

            var cell = await _gridService.PrepareCell(Context.ConnectionId, userId, x, y, gameAction, cropType);

            if (cell != null)
            {
                _cells.Enqueue(cell);
                SetUpdateCellTimer(GetDuration(gameAction));
            }
        }

        public async Task CollectWater(string userId)
        {
            var user = await _userService.GetUserData(new GameUser() { id = userId });
            user.ConnectionId = Context.ConnectionId;
            await _userService.UpdateUser(user);
            if(user.PerformingAction)
            {
                await Clients.Caller.SendAsync("Error", "Cannot collect water while peforming another action!");
                return;
            }    

            if (user != null)
            {
                user.HasWater = true;
                user.PerformingAction = true;
                await _userService.UpdateUser(user);
                _gameUsersCollectingWater.Enqueue(user);
                SetCollectWaterTimer(_durationMs);
            }
        }

        private void SetCollectWaterTimer(double milliseconds)
        {
            // Create a timer with a two second interval.
            _timer = new System.Timers.Timer(milliseconds);
            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += OnWaterCollect;
            _timer.AutoReset = true;
            _timer.Enabled = true;
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
                await _gridService.UpdateCell(c);
                await _hubContext.Clients.All.SendAsync("ReceiveCell", c);
                c.UserId = null;
                await _gridService.UpdateCell(c);
                _cells.Dequeue();
            }
        }

        private async void OnWaterCollect(Object source, ElapsedEventArgs e)
        {
            while (_gameUsersCollectingWater.Count > 0)
            {
                var c = _gameUsersCollectingWater.Peek();
                await _hubContext.Clients.All.SendAsync("ReceiveUser", c);
                _gameUsersCollectingWater.Dequeue();
            }
        }

        private double GetDuration(string action)
        {
            var theAction = action.ToLower();
            switch (theAction)
            {
                case "till":
                        return _durationMs;
                case "sow":
                        return _durationMs;
                case "water":
                        return _durationMs;
                case "harvest":
                    return _durationMs;
                default: return _durationMs;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userService.GetUserByConnectionId(Context.ConnectionId);
            user.PerformingAction = false;
            await _userService.UpdateUser(user);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
