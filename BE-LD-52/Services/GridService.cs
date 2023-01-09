using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace BE_LD_52.Services
{
    public class GridService : IGridService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IUserService _userService;

        public GridService(IConfiguration config, IUserService userService)
        {
            _cosmosClient = new CosmosClient(connectionString: config.GetSection("Cosmos").Value);
            _userService = userService;
        }

        public async Task<Cell> GetCellInfo(int x, int y)
        {
            var container = _cosmosClient.GetContainer("griddatabase", "gridcontainer");

            try
            {
                var cellId = $"{x}|{y}";
                var cell = await container.ReadItemAsync<Cell>(cellId, partitionKey: new PartitionKey(cellId));
                return cell;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task InitializeGrid(int width, int height)
        {
            var container = _cosmosClient.GetContainer("griddatabase", "gridcontainer");

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Random rnd = new Random();
                    var next = rnd.Next(10);
                    var newCellState = "";
                    if (next > 8)
                    {
                        newCellState = "water";
                    }
                    else
                    {
                        newCellState = "raw";
                    }

                    var newCell = new Cell()
                    {
                        id = $"{j}|{i}",
                        X = j,
                        Y = i,
                        State = newCellState
                    };
                    var grid = await container.UpsertItemAsync(newCell, new PartitionKey(newCell.id));
                }
            }

        }

        public async Task<GridInfo> GetGrid()
        {
            var container = _cosmosClient.GetContainer("griddatabase", "gridcontainer");

            try
            {
                var q = container.GetItemLinqQueryable<Cell>();
                var iterator = q.ToFeedIterator();
                var results = await iterator.ReadNextAsync();
                var coords = results.Last().id.Split("|");
                var gridInfo = new GridInfo()
                {
                    Width = Int32.Parse(coords[0]) + 1,
                    Height = Int32.Parse(coords[1]) + 1,
                    Grid = results.ToList()
                };
                
                return gridInfo;
            }
            catch(Exception ex)
            {
                return null;
            }

            return new GridInfo();
        }

        public async Task<Cell> PrepareCell(string userId, int x, int y, string gameAction, string? cropType)
        {
            var cellId = $"{x}|{y}";

            var getCell = await GetCellInfo(x, y);

            //If userid is populated someone "owns" the cell
            if (getCell.UserId != null)
                return null;

            if (gameAction == null) return getCell;

            var cellNextState = VerifyNextState(getCell, gameAction);

            //guard against same action on cell
            if (getCell.State == cellNextState)
                return getCell;


            if (gameAction.ToLower() == "sow")
            {
                getCell.CropType = cropType;
                getCell.CropValue = GetHarvestValue(cropType);
            }

            if(gameAction.ToLower() == "water")
            {
                var user = await _userService.GetUserData(new GameUser() { id = userId });
                user.HasWater = false;
                await _userService.UpdateUser(user);
            }

            var cell = new Cell()
            {
                id = getCell.id,
                X = x,
                Y = y,
                State = cellNextState,
                CropType = getCell.CropType,
                CropValue = getCell.CropValue,
                UserId = userId
            };

            return cell;
        }

        public async Task<Cell> UpdateCell(Cell cell)
        {
            var container = _cosmosClient.GetContainer("griddatabase", "gridcontainer");

            try
            {
                //free up cell but also need to add cooldown
                cell.UserId = null;
                return await container.UpsertItemAsync<Cell>(cell, partitionKey: new PartitionKey(cell.id));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string VerifyNextState(Cell cell, string gameAction)
        {
            var state = cell.State.ToLower();
            var theAction = gameAction.ToLower();
            switch (state)
            {
                case "raw":
                    if (theAction == "till")
                        return ("tilled");
                    break;
                case "tilled":
                    if (theAction == "sow")
                        return ("sown");
                    break;
                case "sown":
                    if (theAction == "water")
                        return ("grown");
                    break;
                case "grown":
                    if (theAction == "harvest")
                        return ("raw");
                    break;
            }

            return cell.State;
        }

        private int GetHarvestValue(string cropType)
        {
            var crop = cropType.ToLower();
            switch(crop)
            {
                case "wheat":
                    return 1;
                case "vegetable":
                    return 2;
                case "fruit":
                    return 3;
                default: return 0;
            }
        }
    }
}
