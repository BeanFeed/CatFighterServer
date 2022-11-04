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
        Console.Clear();
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
        if(!devMode)
        {
            Server(port);
        }
        else
        {
            int selected = 0;
            bool optSel = false;
            ConsoleColor defBack = Console.BackgroundColor;
            ConsoleColor defFor = Console.ForegroundColor;

            while(!optSel)
            {
                Console.BackgroundColor = defBack;
                Console.ForegroundColor = defFor;
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.Write("Developer Testing Screen");
                Console.SetCursorPosition(0,1);
                if(selected == 0)
                {
                    Console.BackgroundColor = defFor;
                    Console.ForegroundColor = defBack;
                    Console.WriteLine("Run Server");
                    Console.BackgroundColor = defBack;
                    Console.ForegroundColor = defFor;
                    Console.Write("Run Test Client");
                }
                else if(selected == 1)
                {
                    Console.WriteLine("Run Server");
                    Console.BackgroundColor = defFor;
                    Console.ForegroundColor = defBack;
                    Console.Write("Run Test Client");
                    Console.BackgroundColor = defBack;
                    Console.ForegroundColor = defFor;
                }
                ConsoleKey key = Console.ReadKey().Key;
                if (key == ConsoleKey.UpArrow && selected != 0) selected--;
                else if (key == ConsoleKey.DownArrow && selected != 1) selected++;
                else if (key == ConsoleKey.Enter) optSel = true;
            }
            if (selected == 0) Server(port);
            else TestClient();
        }

    }

    static void Server(int port)
    {

        Message data;
        //Creates New UdpClient
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
        UdpClient udp = new UdpClient(ipep);
        
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        List<Client> clients = new List<Client>();
        while(true)
        {
            string msg = Encoding.ASCII.GetString(udp.Receive(ref sender));
            if (msg != "")
            {
                data = JsonConvert.DeserializeObject<Message>(msg);
                //If Message is connect or disconnect add or remove from client list
                if (data.messageType == Message.MessageType.ServerConnect || data.messageType == Message.MessageType.ServerDisconnect)
                {
                    if (data.messageType == Message.MessageType.ServerDisconnect) clients.Remove(data.sender);
                    else
                    {
                        //Adds client to client list and sends client their IPEndPoint
                        clients.Add(data.sender);
                        Message toSend = new Message();
                        data.sender.ipep = sender;
                        toSend.messageType = Message.MessageType.ServerConnect;
                        toSend.message = "";
                        toSend.toChatId = data.sender.chatId;
                        toSend.sender = data.sender;
                        udp.SendAsync(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(toSend)));
                    }
                }
                else
                {
                    foreach (Client cl in clients)
                    {
                        if (cl.chatId == data.toChatId)
                        {
                            udp.SendAsync(Encoding.ASCII.GetBytes(msg), cl.ipep);
                        }
                    }
                }
            }
            msg = "";
            
        }
    }
    static bool OnlyInt(string input)
    {
        string ints = "0123456789";
        bool containsChar = false;
        for(int i = 0; i < input.Length; i++)
        {
            if (!ints.Contains(input[i])) containsChar = true;
        }
        return !containsChar;
    }

    static void TestClient()
    {
        //Gets the Ip Address and Port of server to connect to
        //TODO: Need to do Null checking/preventing later
        Console.Clear();
        Console.SetCursorPosition(0, 0);
        Console.Write("Server Ip: ");
        string ip = Console.ReadLine();
        Console.Write("Port[4477]: ");
        string sPort = Console.ReadLine();
        int port = 4477;
        //Makes sure port isn't blank and is a number
        if (sPort != "" && OnlyInt(sPort)) port = int.Parse(sPort);
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        UdpClient udp = new UdpClient(ipEndPoint);
        Client self = new Client();
        self.id = new Random().Next(100000,999999);
        self.chatId = GenRandString();
        //Send message to server to get self IPEndpoint
        Message msg = new Message();
        msg.sender = self;
        msg.toChatId = self.chatId;
        msg.messageType = Message.MessageType.ServerConnect;
        msg.message = "";
        //Ascii gui to select whether to send or recieve messages
        int selected = 0;
        bool optSel = false;
        ConsoleColor defBack = Console.BackgroundColor;
        ConsoleColor defFor = Console.ForegroundColor;
        
        while (!optSel)
        {
            Console.BackgroundColor = defBack;
            Console.ForegroundColor = defFor;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.Write("Developer Testing Screen");
            Console.SetCursorPosition(0, 1);
            if (selected == 0)
            {
                Console.BackgroundColor = defFor;
                Console.ForegroundColor = defBack;
                Console.WriteLine("Send Messages");
                Console.BackgroundColor = defBack;
                Console.ForegroundColor = defFor;
                Console.Write("Recieve Messages");
            }
            else if (selected == 1)
            {
                Console.WriteLine("Send Messages");
                Console.BackgroundColor = defFor;
                Console.ForegroundColor = defBack;
                Console.Write("Recieve Messages");
                Console.BackgroundColor = defBack;
                Console.ForegroundColor = defFor;
            }
            ConsoleKey key = Console.ReadKey().Key;
            if (key == ConsoleKey.UpArrow && selected != 0) selected--;
            else if (key == ConsoleKey.DownArrow && selected != 1) selected++;
            else if (key == ConsoleKey.Enter) optSel = true;
        }
        if(selected == 0)
        {
            //Send Messages
        }
        else
        {
            //Recieve Messages
        }

    }
    static string GenRandString()
    {
        string chars = "abcdefghijklmnopqrstuvwxyz";
        string outS = "";
        for (int i = 0; i < 5; i++)
        {
            outS += chars[new Random().Next(0, chars.Length - 1)];
        }
        return outS;
    }
}


class Client
{
    public IPEndPoint ipep { get; set; }
    public int id { get; set; }
    public string chatId { get; set; }
}
class Message
{
    public enum MessageType
    {
        GameStateUpdate,
        PlayerPositionUpdate,
        PlayerActionUpdate,
        PlayerMessage,
        ServerConnect,
        ServerDisconnect
    }
    public Client sender { get; set; }
    public MessageType messageType { get; set; }
    public string message { get; set; }
    public string toChatId { get; set; }
}