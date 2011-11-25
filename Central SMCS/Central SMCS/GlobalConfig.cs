using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Central_SMCS
{
    class GlobalConfig
    {
        public static Dictionary<String, String> Get()
        {
            Dictionary<String, String> config = new Dictionary<string, string>();

            string globalConfigLocation = (string)System.Configuration.ConfigurationManager.AppSettings["GlobalConfigLocation"];
            StreamReader reader = new StreamReader(globalConfigLocation);

            while (!reader.EndOfStream)
            {
                string[] data = Regex.Split(reader.ReadLine(), ": ");

                config.Add(data[0], data[1]);
            }
            reader.Close();
            reader.Dispose();

            return config;
        }
    }
}
