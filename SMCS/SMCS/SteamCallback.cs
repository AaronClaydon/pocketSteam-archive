using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using Newtonsoft.Json;

namespace SMCS
{
    class SteamCallback : ICallbackHandler
    {
        DatabaseEntities db = new DatabaseEntities();

        public void HandleCallback(CallbackMsg msg)
        {
            if (msg.IsType<SteamFriends.PersonaStateCallback>())
            {
                var perState = (SteamFriends.PersonaStateCallback)msg;

                if (perState.AvatarHash != null && !IsZeros(perState.AvatarHash))
                {
                    CDNCache.DownloadAvatar(perState.FriendID, perState.AvatarHash, new Action<AvatarDownloadDetails>(delegate(AvatarDownloadDetails details) { }));
                }

                //Add logged in user data
                if (perState.FriendID == Steam3.SteamUser.GetSteamID())
                {
                    byte[] avatarHash = CDNCache.GetAvatarHash(perState.FriendID);
                    string avatarUrl = "http://media.steampowered.com/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg";

                    if (avatarHash != null)
                        avatarUrl = CDNCache.GetAvatarUrl(avatarHash);

                    string playerState = Steam3.SteamFriends.GetPersonaState().ToString();
                    string gamePlayedName = Steam3.SteamFriends.GetFriendGamePlayedName(Steam3.SteamUser.GetSteamID());
                    if (!String.IsNullOrEmpty(gamePlayedName))
                        playerState = "Playing " + gamePlayedName;
                    else
                        playerState = Steam3.SteamFriends.GetPersonaState().ToString();

                    SteamUserData messageObject = new SteamUserData
                    {
                        SteamID = Steam3.SteamUser.GetSteamID().ToString(),
                        SteamName = Steam3.SteamFriends.GetPersonaName(),
                        AvatarURL = avatarUrl,
                        State = playerState
                    };
                    string messageJson = JsonConvert.SerializeObject(messageObject);

                    Message message = new Message
                    {
                        SessionToken = Program.sessionToken,
                        Type = 1,
                        MessageValue = messageJson,
                        DateCreated = DateTime.Now
                    };
                    db.Messages.AddObject(message);
                    db.SaveChanges();
                }
                else
                {
                    //Dictionary<String, SteamUserData> friends = new Dictionary<string, SteamUserData>();
                    List<SteamUserData> friends = new List<SteamUserData>();

                    for (int i = 0; i < Steam3.SteamFriends.GetFriendCount(); i++)
                    {
                        SteamID friendID = Steam3.SteamFriends.GetFriendByIndex(i);

                        byte[] avatarHash = CDNCache.GetAvatarHash(friendID);
                        string avatarUrl = "http://media.steampowered.com/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg";

                        if (avatarHash != null)
                            avatarUrl = CDNCache.GetAvatarUrl(avatarHash);
                        
                        string playerState = Steam3.SteamFriends.GetFriendPersonaState(friendID).ToString();
                        string gamePlayedName = Steam3.SteamFriends.GetFriendGamePlayedName(friendID);

                        int stateID = 99;
                        switch (Steam3.SteamFriends.GetFriendPersonaState(friendID))
                        {
                            case EPersonaState.Online:
                                stateID = 2;
                                break;
                            case EPersonaState.Away:
                                stateID = 3;
                                break;
                            case EPersonaState.Busy:
                                stateID = 4;
                                break;
                            case EPersonaState.Snooze:
                                stateID = 5;
                                break;
                            case EPersonaState.Offline:
                                stateID = 6;
                                break;
                        }

                        if (!String.IsNullOrEmpty(gamePlayedName))
                        {
                            playerState = "Playing " + gamePlayedName;
                            stateID = 1;
                        }
                        else
                            playerState = Steam3.SteamFriends.GetFriendPersonaState(friendID).ToString();

                        SteamUserData messageObject = new SteamUserData
                        {
                            SteamID = friendID.ToString(),
                            SteamName = Steam3.SteamFriends.GetFriendPersonaName(friendID),
                            AvatarURL = avatarUrl,
                            State = playerState,
                            StateID = stateID
                        };

                        //friends.Add(friendID.ToString(), messageObject);
                        friends.Add(messageObject);
                    }

                    //Delete other friends lists in DB
                    List<Message> queuedMessages = db.Messages.Where(d => d.Type == 4).ToList();
                    foreach (Message toDeleteMsg in queuedMessages)
                    {
                        db.Messages.DeleteObject(toDeleteMsg);
                    }

                    db.SaveChanges();

                    friends = friends.OrderBy(d => d.StateID).ThenBy(d => d.SteamName).ToList();
                    string messageJson = JsonConvert.SerializeObject(friends);

                    Message message = new Message
                    {
                        SessionToken = Program.sessionToken,
                        Type = 4,
                        MessageValue = messageJson,
                        DateCreated = DateTime.Now
                    };
                    db.Messages.AddObject(message);
                    db.SaveChanges();

                    Program.FriendsSent++;

                    Console.Title = "SMCS / " + Program.userName + " / " + Program.FriendsSent + " / Port: " + Program.portNumber;
                }
            }

            if (msg.IsType<SteamUser.LoggedOffCallback>())
            {
                var callback = (SteamUser.LoggedOffCallback)msg;

                Program.ShutDown("you were logged off");

                return;
            }

            if (msg.IsType<SteamFriends.FriendsListCallback>())
            {
                //selfControl.SetSteamID(new Friend(Steam3.SteamUser.GetSteamID()));
                Steam3.SteamFriends.SetPersonaState(EPersonaState.Online);
            }

            if (msg.IsType<SteamUser.LogOnCallback>())
            {
                var logOnResp = (SteamUser.LogOnCallback)msg;

                if (logOnResp.Result == EResult.OK)
                {
                    try
                    {
                        Session session = db.Sessions.Single(d => d.SessionToken == Program.sessionToken);
                        session.Status = 2;
                        db.SaveChanges();

                        Program.steamConnectionReply = "Success";
                    }
                    catch 
                    {
                        Environment.Exit(0);
                    }
                }
                else if (logOnResp.Result == EResult.InvalidPassword)
                {
                    Steam3.Shutdown();
                    Program.steamConnectionReply = "Invalid";
                    Steam3.RemoveHandler(this);
                    Program.ShutDown("SteamGuard");
                }
                else if (logOnResp.Result == EResult.AccountLogonDenied || logOnResp.Result == EResult.InvalidLoginAuthCode)
                {
                    Steam3.Shutdown();
                    Program.steamConnectionReply = "SteamGuard";
                    Steam3.RemoveHandler(this);
                    Program.ShutDown("SteamGuard");
                }
                else if (logOnResp.Result == EResult.AlreadyLoggedInElsewhere)
                {
                    Steam3.Shutdown();
                    Program.steamConnectionReply = "LoggedInElsewhere";
                    Steam3.RemoveHandler(this);
                    Program.ShutDown("LoggedInElseWhere");
                }
                else if (logOnResp.Result != EResult.OK)
                {
                    Steam3.Shutdown();
                    Program.steamConnectionReply = "UnknownConnectFail " + logOnResp.Result;
                    Steam3.RemoveHandler(this);
                    Program.ShutDown("UnknownConnectFail");
                }
            }

            if (msg.IsType<SteamUser.LoginKeyCallback>())
            {
                Steam3.SteamFriends.SetPersonaState(EPersonaState.Online);
            }

            if (msg.IsType<SteamFriends.FriendAddedCallback>())
            {
                var friendAdded = (SteamFriends.FriendAddedCallback)msg;

                if (friendAdded.Result != EResult.OK)
                {
                    Program.ShutDown("friend added error");
                }
            }
            try
            {
                msg.Handle<SteamClient.DisconnectCallback>((callback) =>
                {
                    Program.ShutDown("disconnect callback");

                    return;
                });
            }
            catch { }
        }

        public static bool IsZeros(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                    return false;
            }
            return true;
        }
    }
}
