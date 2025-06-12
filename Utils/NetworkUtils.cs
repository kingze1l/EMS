using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace EMS.Utils
{
    public static class NetworkUtils
    {
        public static string GetLocalIpAddress()
        {
            string localIP = "Unknown";
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch { }
            return localIP;
        }
    }
} 