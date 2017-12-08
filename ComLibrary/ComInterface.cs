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
        void sendMove(Movement move);
        void sendCoinEaten(int playerNumber, string coinName);
        void sendPlayerDead(int playerNumber);
        


    }
    public interface IClient
    {
        void movePlayer(int roundID, string players_arg);
        void moveGhost(int roundID, string monster_arg);
        void coinEaten(int playerNumber, string coinName);
        void playerDead(int playerNumber);
        void startGame(int playerNumber, string arg);
        void receiveRoundUpdate(BoardInfo board);

        void broadcast(int id, string nick, string msg);
        void send(string nick, string msg, int mId);
        void askMessage(string nick, int id);
        int getId();
        void sendLider(int next);
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
        void Freeze();
        void Unfreeze();
        void InjectDelay(string pid2);
        void newServerCreated(string servername, string serverURL);
        BoardInfo getLocalState(int roundID);
    }
    public interface IServerReplication
    {
        void connectServer(string nick, string url);
        void receiveServer(string nick, string url, BoardInfo board, List<ConnectedClient> clients);
        void UpdateBoard(BoardInfo board);
    }

    //TODO not applied yet
    public interface IremotingException
    {
        void MyException(SerializationInfo info, StreamingContext context);
    }
}