using BE_LD_52.Models;

namespace BE_LD_52.Services.Interfaces
{
    public interface IUserService
    {
        public Task<GameUser> GetUserData(GameUser gameUser);
        public Task<GameUser> UpdateUser(GameUser gameUser);
        public Task<List<GameUser>> GetLeaderboard();
    }
}
