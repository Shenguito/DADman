using ComLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class ThreadMove
    {
        public void add(ref List<Movement>listMove, Movement move)
        {
            lock (this)
            {
                listMove.Add(move);
                if (listMove.Count == 1)
                {
                    Monitor.Pulse(this);
                }
            }
        }

        public void ret(ref List<Movement> listMove, ref int roundId, ref bool dead, ref bool sent, ref Dictionary<string, IServer> serversConnected)
        {
            lock (this)
            {
                if (sent)
                {
                    return;
                }
                while (listMove.Count == 0)
                {
                    Monitor.Wait(this);
                }
                Movement move = listMove[0];
                listMove.RemoveAt(0);
                move.roundID = roundId;
                if (!dead)
                {
                    foreach (KeyValuePair<string, IServer> entry in serversConnected)
                    {
                        if (move.move.Equals("LEFT"))
                        {
                            entry.Value.sendMove(move);
                            sent = true;
                        }
                        else if (move.move.Equals("RIGHT"))
                        {
                            entry.Value.sendMove(move);
                            sent = true;
                        }
                        else if (move.move.Equals("UP"))
                        {
                            entry.Value.sendMove(move);
                            sent = true;
                        }
                        else if (move.move.Equals("DOWN"))
                        {
                            entry.Value.sendMove(move);
                            sent = true;
                        }
                    }
                }
            }
        }
    }
}
