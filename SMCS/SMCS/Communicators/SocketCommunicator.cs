using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SMCS
{
    class SocketCommunicator : CommonCommunicator
    {
        TcpListener server;
        NetworkStream currentClientStream;
        int clientNumber = 0;
        double lastHeartbeat = 0;

        public override void Initiate()
        {
            Console.WriteLine("Socket communicator");
            server = new TcpListener(IPAddress.Any, portNumber);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(ProcessClient));
                clientThread.Start(client);
            }
        }

        void ProcessClient(object objClient)
        {
            Console.WriteLine("New client connected");
            TcpClient client = (TcpClient)objClient;
            NetworkStream clientStream = client.GetStream();
            this.currentClientStream = clientStream; //Update to new client
            this.clientNumber += 1;

            byte[] bytes = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(bytes, 0, bytes.Length);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                string message = ASCIIEncoding.ASCII.GetString(bytes, 0, bytesRead);

                if (message == "RepeatSteamReply")
                {
                    while (Program.steamConnectionReply == "") { } //Block it until it has a proper value!
                    SendRawMessage(client, Program.steamConnectionReply);
                    break;
                }
                else if (message == "HEART")
                {
                    lastHeartbeat = CommonCommunicator.UnixTime();
                    byte[] heartbeat = ASCIIEncoding.ASCII.GetBytes("BEAT");
                    clientStream.Write(heartbeat, 0, heartbeat.Length);
                    clientStream.Flush();
                }
                else
                    Program.ProcessCommand(message);
            }

            Console.WriteLine("Client disconnected");
            client.Close();
        }

        void SendRawMessage(TcpClient Client, string Message)
        {
            byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(Message);
            Client.GetStream().Write(sendBytes, 0, sendBytes.Length);
        }

        public override void SendClientMessage(int Type, string MessageValue)
        {
            if (this.clientNumber >= 2)
            {
                SocketMessage message = new SocketMessage
                {
                    Type = Type,
                    MessageValue = MessageValue
                }; //holy confusing code batman!
                string messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message);

                byte[] bytes = ASCIIEncoding.ASCII.GetBytes(messageJson);
                currentClientStream.Write(bytes, 0, bytes.Length);
                currentClientStream.Flush();

                if (Type == 4)
                    Console.WriteLine("Friends list sent");
            }
        }

        public override double GetLastHeartBeat()
        {
            return CommonCommunicator.UnixTime() - lastHeartbeat;
        }
    }
}
