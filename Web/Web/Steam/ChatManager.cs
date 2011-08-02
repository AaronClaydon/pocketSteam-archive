using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;

namespace Web
{
    class ChatManager : ICallbackHandler
    {
        public ChatManager()
        {
            Steam3.AddHandler( this );
        }


        public void HandleCallback( CallbackMsg msg )
        {
            if ( !msg.IsType<SteamFriends.FriendMsgCallback>() )
                return;

            var friendMsg = ( SteamFriends.FriendMsgCallback )msg;

            EChatEntryType type = friendMsg.EntryType;

            if ( type == EChatEntryType.ChatMsg || type == EChatEntryType.Emote )
            {
                /*
                string messageText = "OFC " + Steam3.SteamFriends.GetFriendPersonaName(friendMsg.Sender) + "!";
                Steam3.SteamFriends.SendChatMessage(friendMsg.Sender, EChatEntryType.ChatMsg, messageText);
                 */
            }
        }
    }
}
