using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMCS
{
    public class Command
    {
        public int Type;
        public string CommandValue;
    }

    public class FriendMessageSend
    {
        public string To;
        public string Message;
    }

    class FriendMessageData
    {
        public string SteamID;
        public string SteamName;
        public string Message;
    }

    class SteamUserData
    {
        public string SteamID;
        public string SteamName;
        public string AvatarURL;
        public int StateID;
        public string State;
    }
}
