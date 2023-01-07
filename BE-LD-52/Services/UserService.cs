using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace BE_LD_52.Services
{
    public class UserService: IUserService
    {
        private readonly CosmosClient _cosmosClient;

        public UserService()
        {
            _cosmosClient = new CosmosClient(
                connectionString: "AccountEndpoint=https://ld52.documents.azure.com:443/;AccountKey=BUiM1hdx5QOdppghIb4v27278g1Vuzj9XlUjsNVQilgP8YdSGVaG1S4rwzGDzgXrISFuwbZZpZo6ACDb3QQTfA==;"
            );
        }

        public async Task<GameUser> GetUserData(GameUser gameUser)
        {
            try
            {
                var container = _cosmosClient.GetContainer("userdatabase", "usercontainer");
                if (gameUser.Name == null)
                {
                    var user = await container.ReadItemAsync<GameUser>(gameUser.id, partitionKey: new PartitionKey(gameUser.id));
                    return user;
                }
                if(gameUser.Name != null && gameUser.id != null)
                {
                    var user = await container.CreateItemAsync(gameUser, new PartitionKey(gameUser.id));
                    return user;
                }
                else
                    throw new Exception("Missing user info: please provide an id and name");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
