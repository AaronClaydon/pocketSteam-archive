using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace SMCS
{
    class GlobalConfig
    {
        public static Dictionary<String, String> Get()
        {
            Dictionary<String, String> config = new Dictionary<string, string>();

            string globalConfigLocation = (string)System.Configuration.ConfigurationManager.AppSettings["GlobalConfigLocation"];
            TextReader reader = new StreamReader(globalConfigLocation);

            string rawConfig = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            string[] lines = Regex.Split(rawConfig, "\r\n");

            foreach (string line in lines)
            {
                string[] data = Regex.Split(line, ": ");

                config.Add(data[0], data[1]);
            }

            return config;
        }
    }
}
