using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ComLibrary
{
    public class Util
    {
        public static string PROJECT_ROOT = @".." +
                            Path.DirectorySeparatorChar + ".." +
                            Path.DirectorySeparatorChar + ".." +
                            Path.DirectorySeparatorChar;
        public static string EXE_PATH = Path.DirectorySeparatorChar +
                            "bin" + Path.DirectorySeparatorChar +
                            "Debug" + Path.DirectorySeparatorChar;


        public static string PCS = "PCS";
        public static string SERVER = "SERVER";
        public static string CLIENT = "CLIENT";
        public static int portPCS = 11000;

        public static string MakeUrl(string protocol, string ip, string port,
            string path)
        {
            return protocol + "://" + ip + ":" + port + "/" + path;
        }

        public static string[] DismakeUrl(string url)
        {
            string[] value = new string[4];
            value[0] = url.Split(':')[0];
            value[1] = url.Split(':')[1].Replace("//", "");
            value[2] = url.Split(':')[2].Split('/')[0];
            value[3] = url.Split(':')[2].Split('/')[1];
            return value;
                
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
        }

        public static string ExtractPortFromURL(string url)
        {
            return new Uri(url).Port.ToString();
        }

        public static string ExtractIPFromURL(string url)
        {
            return new Uri(url).Host;
        }
    }
}
