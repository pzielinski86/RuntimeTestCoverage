using System;

namespace Core
{
    public interface ICodeBattlesService
    {
        TankBase DownloadBot(int playerId);
        void DownloadBotAsync(int playerId, Action<TankBase,string> callback);
       
    }
}
