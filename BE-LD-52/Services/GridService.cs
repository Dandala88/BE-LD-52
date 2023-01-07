using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.Azure.Cosmos;

namespace BE_LD_52.Services
{
    public class GridService : IGridService
    {
        private readonly CosmosClient _cosmosClient;

        public GridService(IConfiguration config)
        {
            _cosmosClient = new CosmosClient(connectionString: config.GetSection("Cosmos").Value);
        }

        public async Task InitializeGrid(int width, int height)
        {
            var container = _cosmosClient.GetContainer("griddatabase", "gridcontainer");

            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    var newCell = new Cell()
                    {
                        id = $"{i}|{j}",
                        User = null,
                        State = "Raw"
                    };
                    var grid = await container.CreateItemAsync(newCell, new PartitionKey(newCell.id));
                }
            }

        }
    }
}
