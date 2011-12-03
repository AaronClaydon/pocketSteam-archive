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
        public static string connectionString;

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

        public static Int64 CountCommand(MySqlCommand Command)
        {
            MySqlConnection dbcon = new MySqlConnection(connectionString);
            dbcon.Open();
            Command.Connection = dbcon;
            Int64 rows = (Int64)Command.ExecuteScalar();

            dbcon.Close();
            dbcon.Dispose();
            Command.Dispose();

            return rows;
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
    }

    class Session
    {
        public int DateCreated { get; set; }
        public int LastHeartbeat { get; set; }
    }
}
