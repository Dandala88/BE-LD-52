﻿using BE_LD_52.Hubs;
using BE_LD_52.Models;
using BE_LD_52.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace BE_LD_52.Services
{
    public class UserService: IUserService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IHubContext<ChatHub> _hubContext;


        public UserService(IConfiguration config, IHubContext<ChatHub> hubContext)
        {
            _cosmosClient = new CosmosClient(connectionString: config.GetSection("Cosmos").Value);
            _hubContext = hubContext;
        }

        public async Task<List<GameUser>> GetLeaderboard()
        {
            var container = _cosmosClient.GetContainer("userdatabase", "usercontainer");

            try
            {
                var q = container.GetItemLinqQueryable<GameUser>();
                var iterator = q.ToFeedIterator();
                var getResult = await iterator.ReadNextAsync();

                var results = new List<GameUser>();

                results.AddRange(getResult);
                while (iterator.HasMoreResults)
                {
                    getResult = await iterator.ReadNextAsync();

                    results.AddRange(getResult);
                }
                return results.OrderByDescending(r => r.Currency).Take(10).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<GameUser> GetUserByConnectionId(string connectionId)
        {

            var container = _cosmosClient.GetContainer("userdatabase", "usercontainer");

            try
            {
                var q = container.GetItemLinqQueryable<GameUser>();
                var iterator = q.ToFeedIterator();
                var results = await iterator.ReadNextAsync();
                return results.Where(r => r.ConnectionId == connectionId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<GameUser?> GetUserData(GameUser gameUser)
        {
            if (gameUser.id == null)
                return null;

            var container = _cosmosClient.GetContainer("userdatabase", "usercontainer");

            try
            {
                var user = await container.ReadItemAsync<GameUser>(gameUser.id, partitionKey: new PartitionKey(gameUser.id));
                var leaderboard = await GetLeaderboard();
                await _hubContext.Clients.All.SendAsync("ReceiveLeaderBoard", leaderboard);
                return user;
            }
            catch (CosmosException ex)
            {
                var user = await container.CreateItemAsync(gameUser, new PartitionKey(gameUser.id));
                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<GameUser> UpdateUser(GameUser gameUser)
        {
            var container = _cosmosClient.GetContainer("userdatabase", "usercontainer");

            try
            {
                var user = await container.UpsertItemAsync(gameUser, new PartitionKey(gameUser.id));
                var conUser = await GetUserByConnectionId(gameUser.ConnectionId);
                await _hubContext.Clients.Client(conUser.ConnectionId).SendAsync("ReceiveUser", conUser);
                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
