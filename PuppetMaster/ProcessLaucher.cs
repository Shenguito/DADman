using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class ProcessLaucher
    {
        /*  a unique identiﬁer PID (of type String) via the PCS listening at the url: PCS URL
        * 
        *  The parameters MSEC PER ROUND and NUM PLAYERS specify the time duration of a round
        *  in msecs and the number of players in each game 
        * 
        *  The server shall expose its services at the address SERVER URL
        *  
        *  The client shall expose its services at the address CLIENT URL.
        *  If the optional parameter ﬁlename is speciﬁed,
        *  the client shall feed its actions from the speciﬁed trace ﬁle.
        *  Else, commands are read from keyboard
        * 
        */

            // 
        internal void startClient(string PID, string PCS_URL, string CLIENT_URL, string MSEC_PER_ROUND, string NUM_PLAYERS, string filename)
        {
            
        }

        internal void startServer(string PID, string PCS_URL, string SERVER_URL, string MSEC_PER_ROUND, string NUM_PLAYERS)
        {
            
        }
    }

    class LaunchNode
    {

    }
}
