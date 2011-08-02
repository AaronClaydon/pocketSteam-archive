using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Threading;
using Newtonsoft.Json;

namespace SMCS
{
    class Program
    {
        static Thread steamUpdateThread;

        public static string userName = string.Empty;
        public static string passWord = string.Empty;
        public static string steamGuardKey = string.Empty;
        public static string sessionToken = string.Empty;

        static DatabaseEntities db = new DatabaseEntities();

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
                }
                i++;
            }

            if (userName != string.Empty && passWord != string.Empty)
                Connect(userName, passWord, steamGuardKey);
            else
                return;
        }

        static void Connect(string userName, string passWord, string steamGuardKey)
        {
            CDNCache.Initialize();

            ClientTGT clientTgt = new ClientTGT();
            byte[] serverTgt = new byte[50];
            AuthBlob accRecord = new AuthBlob();

            try
            {
                Steam2.Initialize(userName, passWord, out clientTgt, out serverTgt, out accRecord);
            }
            catch
            {
                Program.ShutDown("can not init");
            }

            try
            {
                Steam3.UserName = userName;
                Steam3.Password = passWord;

                if (steamGuardKey != "" && steamGuardKey != null)
                    Steam3.AuthCode = steamGuardKey;

                Steam3.ClientTGT = clientTgt;
                Steam3.ServerTGT = serverTgt;
                Steam3.AccountRecord = accRecord;

                Steam3.AlternateLogon = false; //Uses PS3 logon

                Steam3.Initialize(true);

                Steam3.Connect();
            }
            catch
            {
                Program.ShutDown("can not init2");
            }
            steamUpdateThread = new Thread(new ThreadStart(SteamUpdate), 1048576);
            steamUpdateThread.Start();

            SteamCallback callback = new SteamCallback();

            Steam3.AddHandler(callback);
        }

        public static void ShutDown(string reason)
        {
            Console.WriteLine("END: " + reason);
            System.IO.TextWriter tr = new System.IO.StreamWriter("debug.txt");
            tr.WriteLine(reason);
            Thread.Sleep(100);
            tr.Close();

            Session session = db.Sessions.Single(d => d.SessionToken == Program.sessionToken);

            Steam3.Shutdown();
            db.Sessions.DeleteObject(session);
            db.SaveChanges();
            Thread.Sleep(500);
            Environment.Exit(0);
        }

        public static void SteamUpdate()
        {
            while (true)
            {
                Steam3.Update();

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
                        /*
                    else if (timeSinceLastHeartbeat.TotalSeconds >= 15 && Steam3.SteamFriends.GetPersonaState() != EPersonaState.Snooze)
                    {
                        Steam3.SteamFriends.SetPersonaState(EPersonaState.Snooze);
                    }*/

                    //Lets loop all friends into a dictionary!
                    Dictionary<String, SteamID> friends = new Dictionary<string, SteamID>();
                    for (int i = 0; i < Steam3.SteamFriends.GetFriendCount(); i++)
                    {
                        SteamID friend = Steam3.SteamFriends.GetFriendByIndex(i);
                        friends.Add(friend.ToString(), friend);
                    }

                    //Get commands
                    List<Command> commands = db.Commands.Where(d => d.SessionToken == Program.sessionToken).ToList();
                    foreach (Command command in commands)
                    {
                        if (command.Type == 1)
                        {
                            db.Commands.DeleteObject(command);
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
                        db.Commands.DeleteObject(command);
                    }

                    db.SaveChanges();
                }
                catch
                {
                    Environment.Exit(0);
                }

                Thread.Sleep(500);
            }
        }
    }
}
