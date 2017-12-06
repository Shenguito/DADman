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
        void connectClient(string nick, string url);
        
        void sendMove(string nick, string direction);
        void sendCoinEaten(int playerNumber, string coinName);
        void sendPlayerDead(int playerNumber);
        


    }
    public interface IClient
    {
        void send(string nick, string msg, int mId);
        void broadcast(int id, string nick, string msg);
        void movePlayer(int roundID, string players_arg, string dead_arg);
        void moveGhost(int roundID, string monster_arg);
        void coinEaten(int playerNumber, string coinName);
        void playerDead(int playerNumber);
        void startGame(int playerNumber, string arg);
        void receiveRoundUpdate(int roundID, string players_arg, string dead_arg, string monster_arg, string coins_arg);
        int getId();
        void sendLider(int next);
        void askMessage(string nick, int id);
        int getClientMessageId();
    }
    public interface IPuppetMasterLauncher
    {
        void LaunchProcess(string name, string args);
        void ExitAllProcesses();
        void crashProcess(string pid);
    }
    public interface IGeneralControlServices
    {
        [OneWay]
        void Freeze();
        void Unfreeze();
        void InjectDelay(string pid1, string pid2);
        void newServerCreated(string servername, string serverURL);
    }
    public interface IServerReplication
    {
        void connectServer(string nick, string url);
        void receiveServer(string nick, string url, int roundid, string players, string monster, string atecoins, string deadplayers);
        void requestRound(int id);
        void UpdateBoard(int roundID, string pl, string monst, string coin, string deadplayers);
    }

    //TODO not applied yet
    public interface IremotingException
    {
        void MyException(SerializationInfo info, StreamingContext context);
    }
}