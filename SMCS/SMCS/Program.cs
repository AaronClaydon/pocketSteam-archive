using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using MySql.Data.MySqlClient;

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
        public static string platform { get; set; }

        public static string steamConnectionReply = "";

        public static int heartbeatFailures = 0;
        public static CommonCommunicator communicator;
       
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
                        communicator.portNumber = Int32.Parse(args[i + 1]); //Platform should always be sent first by CSMCS anyway so this won't error! (hopefully)
                        break;
                    case "-platform":
                        platform = args[i + 1];
                        if (platform == "Web")
                            communicator = new WebCommunicator();
                        else
                            communicator = new SocketCommunicator(); //If its not web then lets just make it a socket one, what else could it be anyway?
                        break;
                }
                i++;
            }

            if (userName != string.Empty && passWord != string.Empty)
            {
                Thread serverThread = new Thread(new ThreadStart(delegate()
                {
                    communicator.Initiate();
                }));
                serverThread.Start();

                Connect(userName, passWord, steamGuardKey);
            }
            else
                return;
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
                Program.Shutdown("can not init2");
            }

            TimeoutCheckThread = new Thread(new ThreadStart(TimeOutCheck));
            TimeoutCheckThread.Name = "Timeout Check";
            TimeoutCheckThread.Start();

            SteamCallback callback = new SteamCallback();
            Steam3.AddHandler(callback);

            Thread.Sleep(200);

            SteamUpdateThread = new Thread(new ThreadStart(SteamUpdate)) { Name = "Steam update" };
            SteamUpdateThread.Start();
        }

        public static void ProcessCommand(string rawCommand)
        {
            //Lets loop all friends into a dictionary for message sending
            Dictionary<String, SteamID> friends = new Dictionary<string, SteamID>();
            for (int i = 0; i < Steam3.SteamFriends.GetFriendCount(); i++)
            {
                SteamID friend = Steam3.SteamFriends.GetFriendByIndex(i);
                friends.Add(friend.ToString(), friend);
            }

            Command command;
            try
            {
                //Actually handle the command
                command = JsonConvert.DeserializeObject<Command>(rawCommand);
            }
            catch
            {
                Console.WriteLine("JSON Parse Exception");
                return;
            }
            if (command.Type == 1)
            {
                Program.Shutdown("Logout request");
            }
            else if (command.Type == 2 || command.Type == 3)
            {
                FriendMessageSend friendMessage;
                try
                {
                    friendMessage = JsonConvert.DeserializeObject<FriendMessageSend>(command.CommandValue);
                }
                catch
                {
                    Console.WriteLine("JSON Parse Exception");
                    return;
                }

                EChatEntryType messageType = EChatEntryType.ChatMsg;
                if (command.Type == 3)
                    messageType = EChatEntryType.Emote; //Both use the same code except this enum

                friendMessage.Message = System.Text.RegularExpressions.Regex.Replace(friendMessage.Message, "&lt;", "<");
                try
                {
                    Steam3.SteamFriends.SendChatMessage(friends[friendMessage.To], messageType, friendMessage.Message);
                }
                catch { }
            }
            else if (command.Type == 4)
            {
                int newStateInt = Int32.Parse(command.CommandValue); //Convert string value to int
                if (newStateInt >= 0 && newStateInt <= 4) //check if its a vlaue usable for a state
                {
                    EPersonaState newState = (EPersonaState)newStateInt;
                    Steam3.SteamFriends.SetPersonaState(newState);
                }
            }
        }

        public static void Shutdown(string reason)
        {
            Console.WriteLine("END: " + reason);
            steamConnectionReply = reason;

            Steam3.Shutdown();
            try
            {
                //Delete session
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "DELETE FROM sessions WHERE SessionToken=@SessionToken";
                command.Parameters.AddWithValue("@SessionToken", Program.sessionToken);

                Database.Command(command);
                command.Dispose();

                //Delete orphaned messages
                command = new MySqlCommand();
                command.CommandText = "DELETE FROM messages WHERE SessionToken=@SessionToken";
                command.Parameters.AddWithValue("@SessionToken", Program.sessionToken);

                Database.Command(command);
                command.Dispose();
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
                try
                {
                    Steam3.Update();
                } catch { }
                Thread.Sleep(1);
            }
        }

        public static void TimeOutCheck()
        {
            while (Update)
            {
                try
                {
                    double timeSinceLastHeartbeat =  communicator.GetLastHeartBeat();
                    if (timeSinceLastHeartbeat >= 30)
                    {
                        //Console.WriteLine(timeSinceLastHeartbeat + " - " + CommonCommunicator.UnixTime());
                        heartbeatFailures++;
                        if (heartbeatFailures >= 2)
                        {
                            Program.Shutdown("timeout");
                            break;
                        }
                    }

                    Console.Title = "SMCS / " + Program.userName + " / Last ping: " + Math.Round(timeSinceLastHeartbeat) + " / Port: " + communicator.portNumber;
                }
                catch (Exception ex)
                {
                    Program.Shutdown("Timeout check error: " + ex.Message);
                }

                Thread.Sleep(10000);
            }
        }
    }
}
