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
        void send(string nick, string msg);
        void broadcast(int id, string nick, string msg);
        void movePlayer(int numberPlayer, string movement);
        void moveGhost(List<int> ghostsMove);
        void coinEaten(int playerNumber, string coinName);
        void playerDead(int playerNumber);
        void startGame(int playerNumber, string arg);
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
    //TODO not applied yet
    public interface IPuppetMasterLauncher
    {
        void LaunchProcess(string name, string args);
    }
}
