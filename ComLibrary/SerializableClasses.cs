using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLibrary
{
    [Serializable]
    public class BoardInfo
    {
        public int RoundID;
        public string Players;
        public string Monsters;
        public string AteCoins;
        public string Coins;
        public string PlayerDead;

        public BoardInfo(int roundId, string Players, string Monsters, string AteCoins, string Coins, string PlayerDead)
        {
            this.RoundID = roundId;
            this.Players = Players;
            this.Monsters = Monsters;
            this.AteCoins = AteCoins;
            this.Coins = Coins;
            this.PlayerDead = PlayerDead;
        }
    }
    [Serializable]
    public class ConnectedClient
    {
        

        public string nick;
        public int playernumber;
        public string url;
        public IClient clientProxy;
        public bool connected;
        public int score;
        public bool dead;
        public ConnectedClient(string nick, int playernumber, string url, IClient clientProxy)
        {
            this.nick = nick;
            this.playernumber = playernumber;
            this.url = url;
            this.clientProxy = clientProxy;
            connected = true;
            score = 0;
            dead = false;
        }
    }
    [Serializable]
    public class Movement
    {
        public string nick;
        public int playernumber;
        public int roundID;
        public string move;
        public bool dead;
        public Movement(int roundID, string nick, int playernumber, string move)
        {
            this.roundID = roundID;
            this.nick = nick;
            this.playernumber = playernumber;
            this.move = move;
        }
    }

}
