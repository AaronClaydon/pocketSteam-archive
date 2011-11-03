using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SMCS
{
    class WebCommunicator : CommonCommunicator
    {
        TcpListener server;
        bool firstConnection = true;

        public override void Initiate()
        {
            Console.WriteLine("Web communicator");
            Console.Write("Starting socket...");
            try
            {
                server = new TcpListener(IPAddress.Any, portNumber);
                server.Start();
            }
            catch
            {
                Program.Shutdown("Server Startup Failed");
                return;
            }
            Console.WriteLine("Done");

            while (Program.Update)
            {
                TcpClient newClient = server.AcceptTcpClient();

                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), newClient);
            }
        }

        void ProcessClient(object clientObject)
        {
            TcpClient client = (TcpClient)clientObject;

            if (!firstConnection)
            {
                byte[] bytes = new byte[1024];
                StringBuilder receivedData = new StringBuilder();

                using (NetworkStream stream = client.GetStream())
                {
                    stream.ReadTimeout = 60000;

                    int bytesRead = 0;
                    try
                    {
                        bytesRead = stream.Read(bytes, 0, bytes.Length);

                        if (bytesRead > 0)
                        {
                            receivedData.Append(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                            stream.ReadTimeout = 10000;
                        }
                    }
                    catch { }
                }

                byte[] repeatBytes = ASCIIEncoding.ASCII.GetBytes("RepeatSteamReply");
                if (bytes == repeatBytes)
                {
                    if (Program.steamConnectionReply != "")
                    {
                        byte[] writeBytes = ASCIIEncoding.ASCII.GetBytes(Program.steamConnectionReply);
                        client.GetStream().Write(writeBytes, 0, writeBytes.Length);
                        client.Close();
                        firstConnection = false;
                    }
                }

                Program.ProcessCommand(receivedData.ToString()); //Send the raw command to the parser!
            }
            else
            {
                while (firstConnection)
                {
                    if (Program.steamConnectionReply != "")
                    {
                        byte[] writeBytes = ASCIIEncoding.ASCII.GetBytes(Program.steamConnectionReply);
                        client.GetStream().Write(writeBytes, 0, writeBytes.Length);
                        client.Close();
                        firstConnection = false;
                    }
                }
            }
        }

        public override void SendClientMessage(int Type, string MessageValue)
        {
            MySql.Data.MySqlClient.MySqlCommand command = new MySql.Data.MySqlClient.MySqlCommand();
            command.CommandText = "INSERT INTO messages (SessionToken, Type, MessageValue, DateCreated) VALUES (@SessionToken, @Type, @MessageValue, @DateCreated);";
            command.Parameters.AddWithValue("@SessionToken", Program.sessionToken);
            command.Parameters.AddWithValue("@Type", Type);
            command.Parameters.AddWithValue("@MessageValue", MessageValue);
            command.Parameters.AddWithValue("@DateCreated", CommonCommunicator.UnixTime());
            Database.Command(command);

            command.Dispose();
        }

        public override double GetLastHeartBeat()
        {
            Session session = Database.GetSession(Program.sessionToken);

            return CommonCommunicator.UnixTime() - session.LastHeartbeat;
        }
    }
}
