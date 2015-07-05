using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.SqlServer
{
    public sealed class BotsRepository : IBotsRepository,IDisposable
    {
        private readonly BotsDataContext _botsDataContext;
        public BotsRepository(string connectionString)
        {
            _botsDataContext = new BotsDataContext(connectionString);
        }

        public void Add(Bot botInfo)
        {
            _botsDataContext.Bots.Add(botInfo);
            _botsDataContext.SaveChanges();
        }

        public IEnumerable<Bot> GetAll()
        {
            return _botsDataContext.Bots;
        }

        public IEnumerable<Bot> GetByName(string name)
        {
            return _botsDataContext.Bots.Where(b => b.Name == name);
        }

        public IEnumerable<Bot> GetUserBots(string userId)
        {
            return _botsDataContext.Bots.Where(b => b.UserId == userId);

        }
        public IEnumerable<Bot> GetEnemyBots(string userId)
        {
            return _botsDataContext.Bots.Where(b => b.UserId != userId);
        }

        public Bot GetById(int id)
        {
            return _botsDataContext.Bots.FirstOrDefault(b => b.Id == id);
        }

        public void Dispose()
        {
            _botsDataContext.Dispose();
        }
    }
}
