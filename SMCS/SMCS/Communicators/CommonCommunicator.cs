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
            Dictionary<String, String> config = GlobalConfig.Get();
            Database.connectionString = 
                "Server=" + config["DB-Host"] + ";" +
                "Database=" + config["DB-Database"] + ";" +
                "User ID=" + config["DB-Username"] + ";" +
                "Password=" + config["DB-Password"] + ";" +
                "Pooling=false";
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
