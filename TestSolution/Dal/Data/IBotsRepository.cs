using System.Collections.Generic;

namespace Data
{
    public interface IBotsRepository
    {
        void Add(Bot botInfo);
        IEnumerable<Bot> GetAll();
        IEnumerable<Bot> GetByName(string name);
        IEnumerable<Bot> GetUserBots(string userId);
        IEnumerable<Bot> GetEnemyBots(string userId);
        Bot GetById(int id);
    }
}
