﻿using BE_LD_52.Models;
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

            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    var newCell = new Cell()
                    {
                        id = $"{i}|{j}",
                        X = i,
                        Y = j,
                        State = "Raw"
                    };
                    var grid = await container.CreateItemAsync(newCell, new PartitionKey(newCell.id));
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
                var t = "";
            }
            catch(Exception ex)
            {

            }

            return new GridInfo();
        }

        public async Task<Cell> UpdateCell(Cell cell)
        {
            var container = _cosmosClient.GetContainer("griddatabase", "gridcontainer");

            try
            {
                var cellId = $"{cell.X}|{cell.Y}";
                cell.id = cellId;
                var newCell = await container.UpsertItemAsync<Cell>(cell, partitionKey: new PartitionKey(cellId));
                return newCell;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
