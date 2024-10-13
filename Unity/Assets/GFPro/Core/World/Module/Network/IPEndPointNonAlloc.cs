using System.Net;

namespace GFPro
{
    public static class EndPointHelper
    {
        public static IPEndPoint Clone(this EndPoint endPoint)
        {
            var ip = (IPEndPoint)endPoint;
            ip = new IPEndPoint(ip.Address, ip.Port);
            return ip;
        }
    }
}