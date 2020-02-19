using CSharpPacheCore;
using CSharpPacheCore.Types;
using System;

namespace ChatService
{
    class Program
    {
        static CPacheService service = null;
        static void Main(string[] args)
        {   
            ServiceConfig config = new ServiceConfig();
             config.IpAdress = "172.31.43.8";
            //config.IpAdress = "10.0.0.160";
            config.ListeningPort = 80;
            service = new CPacheService(config);
            service.Start<Program>();
        }   
    }
}
