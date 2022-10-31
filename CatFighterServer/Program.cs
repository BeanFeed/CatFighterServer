using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace CFS;

class Program
{
    static void Main(string[] args)
    {
        int port = 4477;
        bool devMode = false;
        if(args.Length > 0)
        {
            for(int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-p") { i++; port = int.Parse(args[i]); }
                if (args[i] == "-d") { devMode = true; }
            }
        }

        Server(port);

    }

    static void Server(int port)
    {
        string data = "";
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
        UdpClient udp = new UdpClient(ipep);
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        List<Client> clients = new List<Client>();
        while(true)
        {
            data = Encoding.ASCII.GetString(udp.Receive(ref sender));
            
        }
    }

    static void TestClient()
    {

    }
}

struct Client
{
    public IPEndPoint ipep { get; set; }
    public int id { get; set; }
    public string chatId { get; set; }
}