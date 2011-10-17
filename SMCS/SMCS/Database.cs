using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace SMCS
{
    class Database
    {
        static string connectionString =
             "Server=localhost;" +
             "Database=pocketsteam;" +
             "User ID=pocketsteam;" +
             "Password=82ZczSpM2NvRzXJd;" +
             "Pooling=false";

        public static void Command(MySqlCommand Command)
        {
            MySqlConnection dbcon = new MySqlConnection(connectionString);
            dbcon.Open();
            Command.Connection = dbcon;
            Command.ExecuteNonQuery();

            dbcon.Close();
            dbcon.Dispose();
            Command.Dispose();
        }

        public static void AddMessage(int Type, string MessageJson)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "INSERT INTO messages (SessionToken, Type, MessageValue, DateCreated) VALUES (@SessionToken, @Type, @MessageValue, @DateCreated);";
            command.Parameters.AddWithValue("@SessionToken", Program.sessionToken);
            command.Parameters.AddWithValue("@Type", Type);
            command.Parameters.AddWithValue("@MessageValue", MessageJson);
            command.Parameters.AddWithValue("@DateCreated", Database.UnixTime());
            Database.Command(command);

            command.Dispose();
        }

        public static Session GetSession(string SessionToken)
        {
            MySqlConnection dbcon = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand();
            dbcon.Open();

            command.CommandText = "SELECT * FROM sessions WHERE SessionToken=@SessionToken";
            command.Parameters.AddWithValue("@SessionToken", Program.sessionToken);
            command.Connection = dbcon;
            command.ExecuteNonQuery();

            Session session = new Session();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                session.DateCreated = (int)reader["DateCreated"];
                session.LastHeartbeat = (int)reader["LastHeartbeat"];
            }

            dbcon.Close();
            dbcon.Dispose();
            command.Dispose();

            return session;
        }

        public static double UnixTime()
        {
            DateTime epoch = new DateTime(1970, 1, 1);
            double unixTime = (DateTime.Now - epoch).TotalSeconds - 3600;

            return unixTime;
        }
    }

    class Session
    {
        public int DateCreated { get; set; }
        public int LastHeartbeat { get; set; }
    }
}
