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
                        if (!String.IsNullOrEmpty(gamePlayedName))
                            playerState = "Playing " + gamePlayedName;
                        else
                            playerState = Steam3.SteamFriends.GetFriendPersonaState(friendID).ToString();

                        SteamUserData messageObject = new SteamUserData
                        {
                            SteamID = friendID.ToString(),
                            SteamName = Steam3.SteamFriends.GetFriendPersonaName(friendID),
                            AvatarURL = avatarUrl,
                            State = playerState
                        };

                        //friends.Add(friendID.ToString(), messageObject);
                        friends.Add(messageObject);
                    }
                    friends = friends.OrderBy(d => d.SteamName).ToList();
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
                    }
                    catch 
                    {
                        Environment.Exit(0);
                    }
                }
                else if (logOnResp.Result == EResult.AccountLogonDenied)
                {
                    Program.ShutDown("steamguard deny");

                    Steam3.AuthCode = "9CTTG";

                    // if we got this logon response, we got disconnected, so lets reconnect
                    try
                    {
                        Steam3.Connect();
                    }
                    catch (Steam3Exception ex)
                    {
                        Program.ShutDown("steamguard connect fail");

                        return;
                    }
                }
                else if (logOnResp.Result != EResult.OK)
                {
                    Program.ShutDown("unknown login response");

                    return;
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

            msg.Handle<SteamClient.DisconnectCallback>((callback) =>
            {
                Program.ShutDown("disconnect callback");

                return;
            });
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
