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
        public string SID; //SteamID
        public string N; //Steam name
        public string M; //Message
    }

    class SteamUserData
    {
        public string SID; //SID
        public string N; //Steam name
        public string A; //Avatar URL
        public int StID; //State ID
        public string St; //State
    }
}
