using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;

namespace SMCS
{
    class Program
    {
        static Thread TimeoutCheckThread;
        static Thread SteamUpdateThread;

        public static bool Update = true;

        public static string userName { get; set; }
        public static string passWord { get; set; }
        public static string steamGuardKey { get; set; }
        public static string sessionToken { get; set; }
        public static int portNumber { get; set; }

        public static TcpListener server { get; set; }
        public static bool firstConnection = true;
        public static string steamConnectionReply = "";

        public static int FriendsSent = 0;

        static DatabaseEntities db = new DatabaseEntities();
        
        [STAThread]
        static void Main(string[] args)
        {
            int i = 0;
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-username":
                        userName = args[i + 1];
                        break;
                    case "-password":
                        passWord = args[i + 1];
                        break;
                    case "-authcode":
                        steamGuardKey = args[i + 1];
                        break;
                    case "-sessionToken":
                        sessionToken = args[i + 1];
                        break;
                    case "-port":
                        portNumber = Int32.Parse(args[i + 1]);
                        break;
                }
                i++;
            }

            if (userName != string.Empty && passWord != string.Empty)
            {
                Thread serverThread = new Thread(new ThreadStart(StartServer));
                serverThread.Start();

                Connect(userName, passWord, steamGuardKey);
            }
            else
                return;
        }

        static void StartServer()
        {
            Console.Write("Starting socket...");
            try
            {
                server = new TcpListener(IPAddress.Any, portNumber);
                server.Start();
            }
            catch
            {
                Program.ShutDown("Server Startup Failed");
                return;
            }
            Console.WriteLine("Done");

            while (Update)
            {
                TcpClient newClient = server.AcceptTcpClient();

                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), newClient);
            }
        }

        static void ProcessClient(object clientObject)
        {
            TcpClient client = (TcpClient)clientObject;

            if (!firstConnection)
            {
                byte[] bytes = new byte[1024];
                StringBuilder receivedData = new StringBuilder();

                using (NetworkStream stream = client.GetStream())
                {
                    stream.ReadTimeout = 60000;

                    int bytesRead = 0;
                    try
                    {
                        bytesRead = stream.Read(bytes, 0, bytes.Length);

                        if (bytesRead > 0)
                        {
                            receivedData.Append(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                            stream.ReadTimeout = 10000;
                        }
                    }
                    catch { }
                }
 
                //Lets loop all friends into a dictionary for message sending
                Dictionary<String, SteamID> friends = new Dictionary<string, SteamID>();
                for (int i = 0; i < Steam3.SteamFriends.GetFriendCount(); i++)
                {
                    SteamID friend = Steam3.SteamFriends.GetFriendByIndex(i);
                    friends.Add(friend.ToString(), friend);
                }

                //Actually handle the command
                Command command = JsonConvert.DeserializeObject<Command>(receivedData.ToString());
                if (command.Type == 1)
                {
                    Steam3.Shutdown();
                }
                else if (command.Type == 2)
                {
                    FriendMessageSend friendMessage = JsonConvert.DeserializeObject<FriendMessageSend>(command.CommandValue);
                    friendMessage.Message = System.Text.RegularExpressions.Regex.Replace(friendMessage.Message, "&lt;", "<");
                    try
                    {
                        Steam3.SteamFriends.SendChatMessage(friends[friendMessage.To], EChatEntryType.ChatMsg, friendMessage.Message);
                    }
                    catch { }
                }
                else if (command.Type == 3)
                {
                    FriendMessageSend friendMessage = JsonConvert.DeserializeObject<FriendMessageSend>(command.CommandValue);
                    friendMessage.Message = System.Text.RegularExpressions.Regex.Replace(friendMessage.Message, "&lt;", "<");
                    try
                    {
                        Steam3.SteamFriends.SendChatMessage(friends[friendMessage.To], EChatEntryType.Emote, friendMessage.Message);
                    }
                    catch { }
                }


                Console.WriteLine();
            }
            else
            {
                while(firstConnection)
                {
                    if (steamConnectionReply != "")
                    {
                        byte[] writeBytes = ASCIIEncoding.ASCII.GetBytes(steamConnectionReply);
                        client.GetStream().Write(writeBytes, 0, writeBytes.Length);
                        client.Close();
                        firstConnection = false;
                    }
                }
            }
        }

        static void Connect(string userName, string passWord, string steamGuardKey)
        {
            CDNCache.Initialize();

            try
            {
                Steam3.UserName = userName;
                Steam3.Password = passWord;

                if (steamGuardKey != "" && steamGuardKey != null)
                    Steam3.AuthCode = steamGuardKey;

                Steam3.AlternateLogon = false; //True = Uses PS3 logon

                Steam3.Initialize(true);

                Steam3.Connect();
            }
            catch
            {
                Program.ShutDown("can not init2");
            }

            db.CommandTimeout = 0;
            TimeoutCheckThread = new Thread(new ThreadStart(TimeOutCheck));
            TimeoutCheckThread.Name = "Timeout Check";
            TimeoutCheckThread.Start();

            SteamUpdateThread = new Thread(new ThreadStart(SteamUpdate)) { Name = "Steam update" };
            SteamUpdateThread.Start();

            SteamCallback callback = new SteamCallback();
            Steam3.AddHandler(callback);
        }

        public static void ShutDown(string reason)
        {
            Console.WriteLine("END: " + reason);

            Steam3.Shutdown();
            try
            {
                Session session = db.Sessions.Single(d => d.SessionToken == Program.sessionToken);

                db.Sessions.DeleteObject(session);
                db.SaveChanges();
                db.Dispose();
            }
            catch { }
            Thread.Sleep(500);
            //Console.ReadLine();
            Environment.Exit(0);
        }

        public static void SteamUpdate()
        {
            while (Update)
            {
                Steam3.Update();
                Thread.Sleep(1);
            }
        }

        public static void TimeOutCheck()
        {
            while (Update)
            {
                try
                {
                    Session session = db.Sessions.Single(d => d.SessionToken == Program.sessionToken);

                    db.Refresh(System.Data.Objects.RefreshMode.ClientWins, session);

                    TimeSpan timeSinceLastHeartbeat = (DateTime.Now - session.LastHeartbeat);
                    if (timeSinceLastHeartbeat.TotalSeconds >= 30)
                    {
                        Program.ShutDown("timeout");
                        break;
                    }
                }
                catch
                {
                    Environment.Exit(0);
                }

                Thread.Sleep(10000);
            }
        }
    }
}
