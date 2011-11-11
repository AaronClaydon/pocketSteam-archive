using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using Newtonsoft.Json;
using System.Data;
using MySql.Data.MySqlClient;

namespace SMCS
{
    class SteamCallback : ICallbackHandler
    {
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

                    string steamName = Steam3.SteamFriends.GetPersonaName();
                    if (steamName.Length > 22)
                        steamName = steamName.Substring(0, 22);

                    int stateID = 99;
                    switch (Steam3.SteamFriends.GetPersonaState())
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

                    SteamUserData messageObject = new SteamUserData
                    {
                        SID = Steam3.SteamUser.GetSteamID().ToString(),
                        N = steamName,
                        A = avatarUrl,
                        St = playerState,
                        StID = stateID
                    };
                    string messageJson = JsonConvert.SerializeObject(messageObject);

                    Program.communicator.SendClientMessage(1, messageJson);
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

                        string steamName = Steam3.SteamFriends.GetFriendPersonaName(friendID);
                        if (steamName.Length > 22)
                            steamName = steamName.Substring(0, 22);

                        SteamUserData messageObject = new SteamUserData
                        {
                            SID = friendID.ToString(),
                            N = steamName,
                            A = avatarUrl,
                            St = playerState,
                            StID = stateID
                        };

                        friends.Add(messageObject);
                    }

                    friends = friends.OrderBy(d => d.StID).ThenBy(d => d.N).ToList();
                    string messageJson = JsonConvert.SerializeObject(friends);

                    Program.communicator.SendClientMessage(4, messageJson);
                }
            }

            if (msg.IsType<SteamUser.LoggedOffCallback>())
            {
                SteamUser.LoggedOffCallback callback = (SteamUser.LoggedOffCallback)msg;

                Program.Shutdown("you were logged off");

                return;
            }

            if (msg.IsType<SteamFriends.FriendsListCallback>())
            {
                Steam3.SteamFriends.SetPersonaState(EPersonaState.Online); //Necesary because steam sometimes forgets to send the login key
            }

            if (msg.IsType<SteamUser.LogOnCallback>())
            {
                var logOnResp = (SteamUser.LogOnCallback)msg;

                if (logOnResp.Result == EResult.OK)
                {
                    try
                    {
                        //Update the session with the status! (is this really needed anymore?)
                        MySqlCommand command = new MySqlCommand();
                        command.CommandText = "UPDATE sessions SET Status=@Status WHERE SessionToken=@SessionToken";
                        command.Parameters.AddWithValue("@Status", 2);
                        command.Parameters.AddWithValue("@SessionToken", Program.sessionToken);
                        Database.Command(command);
                        command.Dispose();

                        //Lets update the users section
                        command = new MySqlCommand();
                        command.CommandText = "SELECT COUNT(*) FROM users WHERE Username=@Username";
                        command.Parameters.AddWithValue("@Username", Program.userName);
                        Int64 rows = Database.CountCommand(command);
                        command.Dispose();

                        if (rows == 0)
                        {
                            //New user - lets add 'em
                            double currentTime = CommonCommunicator.UnixTime();
                            int hasSteamGuard = Program.steamGuardKey != "" && Program.steamGuardKey != null ? 1:0; //AMAZING SHORTHAND

                            command = new MySqlCommand();
                            command.CommandText = "INSERT INTO users (Username, SteamID, SteamGuard, TimesLogin, LastLogin, FirstLogin) VALUES (@Username, @SteamID, @SteamGuard, @TimesLogin, @LastLogin, @FirstLogin);";
                            command.Parameters.AddWithValue("@Username", Program.userName);
                            command.Parameters.AddWithValue("@SteamID", Steam3.SteamUser.GetSteamID().ToString());
                            command.Parameters.AddWithValue("@SteamGuard", hasSteamGuard);
                            command.Parameters.AddWithValue("@TimesLogin", 1);
                            command.Parameters.AddWithValue("@LastLogin", currentTime);
                            command.Parameters.AddWithValue("@FirstLogin", currentTime);

                            Database.Command(command);
                            command.Dispose();
                        }
                        else
                        {
                            //Update user
                            double currentTime = CommonCommunicator.UnixTime();

                            command = new MySqlCommand();
                            command.CommandText = "UPDATE users SET TimesLogin=TimesLogin+1, LastLogin=@LastLogin WHERE Username=@Username";
                            command.Parameters.AddWithValue("@Username", Program.userName);
                            command.Parameters.AddWithValue("@LastLogin", currentTime);

                            Database.Command(command);
                            command.Dispose();
                        }

                        Program.steamConnectionReply = "Success";
                    }
                    catch
                    {
                        Program.Shutdown("Can not update DB session");
                    }
                }
                else if (logOnResp.Result == EResult.InvalidPassword)
                {
                    Steam3.Shutdown();
                    Program.steamConnectionReply = "Invalid";
                    Steam3.RemoveHandler(this);
                    Program.Shutdown("Invalid");
                }
                else if (logOnResp.Result == EResult.AccountLogonDenied || logOnResp.Result == EResult.InvalidLoginAuthCode)
                {
                    Steam3.Shutdown();
                    Program.steamConnectionReply = "SteamGuard";
                    Steam3.RemoveHandler(this);
                    Program.Shutdown("SteamGuard");
                }
                else if (logOnResp.Result == EResult.AlreadyLoggedInElsewhere)
                {
                    Steam3.Shutdown();
                    Program.steamConnectionReply = "LoggedInElsewhere";
                    Steam3.RemoveHandler(this);
                    Program.Shutdown("LoggedInElseWhere");
                }
                else if (logOnResp.Result != EResult.OK)
                {
                    Steam3.Shutdown();
                    Program.steamConnectionReply = "UnknownConnectFail " + logOnResp.Result;
                    Steam3.RemoveHandler(this);
                    Program.Shutdown("UnknownConnectFail");
                }
            }

            if (msg.IsType<SteamUser.LoginKeyCallback>())
            {
                Console.WriteLine("Login key received - Setting online");
                Steam3.SteamFriends.SetPersonaState(EPersonaState.Online);
            }

            if (msg.IsType<SteamFriends.FriendAddedCallback>())
            {
                var friendAdded = (SteamFriends.FriendAddedCallback)msg;

                if (friendAdded.Result != EResult.OK)
                {
                    Program.Shutdown("friend added error");
                }
            }
            try
            {
                msg.Handle<SteamClient.DisconnectCallback>((callback) =>
                {
                    Program.Shutdown("disconnect callback");

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
