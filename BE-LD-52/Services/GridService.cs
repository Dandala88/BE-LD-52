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

        public Task InitializeGrid(int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}
