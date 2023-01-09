using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace BE_LD_52.Services
{
    public class GridService : IGridService
    {
        private readonly CosmosClient _cosmosClient;

        public GridService(IConfiguration config)
        {
            _cosmosClient = new CosmosClient(connectionString: config.GetSection("Cosmos").Value);
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

            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    var newCell = new Cell()
                    {
                        id = $"{j}|{i}",
                        X = j,
                        Y = i,
                        State = "raw"
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



        public async Task<Cell> PrepareCell(int x, int y, string gameAction)
        {
            var cellId = $"{x}|{y}";

            var getCell = await GetCellInfo(x, y);

            //If userid is populated someone "owns" the cell
            if (getCell.UserId != null)
                return null;

            if (gameAction == null) return getCell;

            var cellNextState = VerifyNextState(getCell, gameAction);

            if (getCell.State == cellNextState)
                return getCell;

            var cell = new Cell()
            {
                id = getCell.id,
                X = x,
                Y = y,
                State = cellNextState,
                UserId = null //temp
            };

            return cell;
        }

        public async Task<Cell> UpdateCell(Cell cell)
        {
            var container = _cosmosClient.GetContainer("griddatabase", "gridcontainer");

            try
            {
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
                    else if (theAction == "whoops")
                    {
                        return ("whoops");
                    }
                    break;
                case "tilled":
                    if (theAction == "sow")
                        return ("sown");
                    break;
                case "sown":
                    if (theAction == "harvest")
                        return ("raw");
                    break;
            }

            return cell.State;
        }
    }
}
