using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using Newtonsoft.Json;

namespace SMCS
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
                if (friendMsg.Message == "Away")
                    Steam3.SteamFriends.SetPersonaState(EPersonaState.Away);
                else if (friendMsg.Message == "Busy")
                    Steam3.SteamFriends.SetPersonaState(EPersonaState.Busy);
                else if (friendMsg.Message == "Online")
                    Steam3.SteamFriends.SetPersonaState(EPersonaState.Online);
                */
                DatabaseEntities db = new DatabaseEntities();

                int messageType = 2;
                if (type == EChatEntryType.Emote)
                    messageType = 3;


                string steamMessage = System.Text.RegularExpressions.Regex.Replace(friendMsg.Message, "<", "&lt;");
                steamMessage = System.Text.RegularExpressions.Regex.Replace(steamMessage, ">", "&gt;");

                FriendMessageData messageObject = new FriendMessageData
                {
                    SteamID = friendMsg.Sender.ToString(),
                    SteamName = Steam3.SteamFriends.GetFriendPersonaName(friendMsg.Sender),
                    Message = steamMessage
                };
                string messageJson = JsonConvert.SerializeObject(messageObject);

                Message message = new Message
                {
                    SessionToken = Program.sessionToken,
                    Type = messageType,
                    MessageValue = messageJson,
                    DateCreated = DateTime.Now
                };

                db.Messages.AddObject(message);
                db.SaveChanges();
            }
        }
    }
}
