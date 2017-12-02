using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ComLibrary
{
    public interface IServer
    {
        void connect(string nick, string url);
        void sendMove(string nick, string direction);
        void sendCoinEaten(int playerNumber, string coinName);
        void sendPlayerDead(int playerNumber);
        
    }
    public interface IClient
    {
        void send(string nick, string msg, DateTime timestamp, Dictionary<string, int> delayLog);
        void broadcast(int id, string nick, string msg, DateTime timestamp, Dictionary<string, int> delayLog);
        void movePlayer(int roundID, string players_arg, string dead_arg);
        void moveGhost(int roundID, string monster_arg);
        void coinEaten(int playerNumber, string coinName);
        void playerDead(int playerNumber);
        void startGame(int playerNumber, string arg);
        void receiveRoundUpdate(int roundID, string players_arg, string dead_arg, string monster_arg, string coins_arg);
    }
    public interface IPuppetMasterLauncher
    {
        void LaunchProcess(string name, string args);
        void ExitAllProcesses();
        void crashProcess(string pid);
    }

    //TODO not applied yet
    public interface IremotingException
    {
        void MyException(SerializationInfo info, StreamingContext context);
    }
    //TODO not applied yet
    public interface IGeneralControlServices
    {
        [OneWay]
        void Freeze();
        void Unfreeze();
    }
    
    
}
