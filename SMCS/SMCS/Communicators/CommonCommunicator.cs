using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMCS
{
    class CommonCommunicator
    {
        public int portNumber;

        public virtual void Initiate() 
        {
            //Blank as it will be inherited
        }

        public void RawSendMessage(string Message)
        {
            //Blank as it will be inherited
        }

        public virtual void SendClientMessage(int Type, string MessageValue)
        {
            //Blank as it will be inherited
        }

        public virtual double GetLastHeartBeat()
        {
            //Blank as it will be inherited
            return 0;
        }

        public static double UnixTime()
        {
            DateTime epoch = new DateTime(1970, 1, 1);
            double unixTime = (DateTime.Now - epoch).TotalSeconds + Int32.Parse(GlobalConfig.Get()["Timezone-Difference"]);

            return unixTime;
        }
    }
}
