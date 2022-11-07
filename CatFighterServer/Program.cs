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
                        data.sender.ip = sender.Address.ToString();
                        data.sender.port = sender.Port;
                        clients.Add(data.sender);
                        Message toSend = new Message(data.sender, Message.MessageType.ServerConnect, "", data.sender.chatId);
                        string toSendSer = JsonConvert.SerializeObject(toSend);
                        //udp.Connect(IPAddress.Parse(toSend.sender.ip), toSend.sender.port);
                        udp.Send(Encoding.ASCII.GetBytes(toSendSer), new IPEndPoint(IPAddress.Parse(toSend.sender.ip), toSend.sender.port));
                        //udp.Close();
                        //udp = new UdpClient(ipep);
                    }
                }
                else
                {
                    foreach (Client cl in clients)
                    {
                        if (cl.chatId == data.toChatId)
                        {
                            if (data.messageType == Message.MessageType.PlayerMessage && (data.message == "" || data.message == null))
                            udp.SendAsync(Encoding.ASCII.GetBytes(msg), new IPEndPoint(IPAddress.Parse(cl.ip),cl.port));
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
        IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        UdpClient udp = new UdpClient();
        udp.Connect(serverEndpoint);
        Client self = new Client(null, new Random().Next(100000, 999999), GenRandString());
        IPEndPoint sender = new IPEndPoint(serverEndpoint.Address, serverEndpoint.Port);
        //Send message to server to get self IPEndpoint
        Message msg = new Message(self, Message.MessageType.ServerConnect, "", self.chatId);
        byte[] rawMsgToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg));
        udp.Send(rawMsgToSend);
        Message data = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(udp.Receive(ref sender)));
        while(self.id != data.sender.id)
        {
            data = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(udp.Receive(ref sender)));
        }
        self.ip = data.sender.ip;
        self.port = data.sender.port;
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
            Console.Clear();
            //bool cont = true;
            Console.Write("Server ChatID: ");
            //TODO: Do null checking
            string toChatID = Console.ReadLine();
            self.chatId = toChatID;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Chat ID: " + self.chatId);
                Console.WriteLine("ID: " + self.id.ToString());
                
                Console.Write("Message: ");
                //TODO: Do null checking
                string input = Console.ReadLine();
                if (input == "") continue;
                Message toSend = new Message(self, Message.MessageType.PlayerMessage, input, toChatID);
                string d = JsonConvert.SerializeObject(toSend);
                //udp = new UdpClient();
                //udp.Connect(serverEndpoint);
                udp.Send(Encoding.ASCII.GetBytes(d));
            }
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Chat ID: " + self.chatId);
            Console.WriteLine("ID: " + self.id.ToString());
            List<string> messageList = new List<string>();
            while(true)
            {
                
                byte[] rawData = udp.Receive(ref sender);
                Console.Clear();
                Console.WriteLine("Chat ID: " + self.chatId);
                Console.WriteLine("ID: " + self.id.ToString());
                Message message = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(rawData));
                if (message.toChatId != self.chatId) continue;
                if (message.messageType != Message.MessageType.PlayerMessage) continue;
                if (message.sender.id == self.id) continue;
                if (message.message == "" || message.message == null) continue;
                messageList.Add(message.sender.id.ToString() + ": " + message.message);
                if(messageList.Count > 5) messageList.RemoveAt(0);
                foreach(string mesg in messageList)
                {
                    Console.WriteLine(mesg);
                }
            }
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
        return outS.ToUpper();
    }
}


class Client
{
    public string? ip;
    public int port;
    public int id;
    public string chatId;
    public Client(IPEndPoint? ipEP, int userId, string chatID)
    {
        if (ipEP != null)
        {
            ip = ipEP.Address.ToString();
            port = ipEP.Port;
        } 
        id = userId;
        chatId = chatID;
    }
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
    public Client sender;
    public MessageType messageType;
    public string message;
    public string toChatId;
    public Message(Client cl, MessageType mt, string msg, string tochatId)
    {
        sender = cl;
        messageType = mt;
        message = msg;
        toChatId = tochatId;
    }
}