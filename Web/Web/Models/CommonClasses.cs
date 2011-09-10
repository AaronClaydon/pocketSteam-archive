using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
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

    public class JsonReturn
    {
        public int Status;
        public List<Message> Messages;
    }
}