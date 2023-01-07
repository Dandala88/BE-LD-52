using BE_LD_52.Models;

namespace BE_LD_52.Services.Interfaces
{
    public interface IGridService
    {
        public Task InitializeGrid(int width, int height);
        public Task<Cell> GetCellInfo(int x, int y);
        public Task<Cell> UpdateCell(Cell cell);
        public Task<GridInfo> GetGrid();
    }
}
