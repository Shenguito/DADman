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
        void connect(string nick, int port);
        void sendMove(string nick, string direction);
        void sendCoinEaten(int playerNumber, string coinName);
        void sendPlayerDead(int playerNumber);
        
    }
    public interface IClient
    {
        void broadcastClientURL(int playerNumber, string nick, int port);
        void send(string nick, string msg);
        void broadcast(int id, string nick, string msg);
        void movePlayer(int numberPlayer, string movement);
        //TODO interception exception
        void moveGhost(List<int> ghostsMove);
        void coinEaten(int playerNumber, string coinName);
        void playerDead(int playerNumber);
        void startGame(int playerNumbers);
    }

    //not applied yet
    public interface IremotingException
    {
        void MyException(SerializationInfo info, StreamingContext context);
    }

    public interface IGeneralControlServices
    {
        [OneWay]
        void Freeze();
        void Unfreeze();
    }
    
    public interface IPuppetMasterLauncher
    {
        void LaunchProcess(string name, string args);
    }
}
