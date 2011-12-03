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
        static TcpClient currentClient;
        int clientNumber = 0;
        double lastHeartbeat = 0;
        static List<SocketMessage> messages = new List<SocketMessage>();

        public override void Initiate()
        {
            base.Initiate();

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
            currentClient = client; //Update to new client
            this.clientNumber += 1;

            if (this.clientNumber >= 2)
            {
                Thread messagesStartThread = new Thread(new ParameterizedThreadStart(MessagesSend));
                messagesStartThread.Start(this);
            }

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
                    Console.WriteLine("HERAT");
                    lastHeartbeat = CommonCommunicator.UnixTime();
                    byte[] heartbeat = ASCIIEncoding.ASCII.GetBytes("BEAT");
                    try
                    {
                        clientStream.Write(heartbeat, 0, heartbeat.Length);
                        clientStream.Flush();
                    }
                    catch { }
                }
                else
                    Program.ProcessCommand(message);
            }

            Console.WriteLine("Client disconnected");
            client.Close();

            if (clientNumber >= 2)
            {
                Program.Shutdown("ClientDisconnect");
            }
        }

        static void MessagesSend(object objThis)
        {
            SocketCommunicator This = (SocketCommunicator)objThis;

            while (Program.Update)
            {
                if (messages.Count > 0)
                {
                    string messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(messages);
                    messages.Clear();
                    Console.WriteLine("Sending some messages");

                    try
                    {
                        NetworkStream clientStream = currentClient.GetStream();
                        byte[] bytes = ASCIIEncoding.ASCII.GetBytes(messageJson);
                        clientStream.Write(bytes, 0, bytes.Length);
                        clientStream.Flush();
                    }
                    catch
                    {
                        return;
                    }
                }

                Thread.Sleep(200);
            }
        }

        void SendRawMessage(TcpClient Client, string Message)
        {
            byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(Message);
            Client.GetStream().Write(sendBytes, 0, sendBytes.Length);
        }

        public override void SendClientMessage(int Type, string MessageValue)
        {
            SocketMessage message = new SocketMessage
            {
                Type = Type,
                MessageValue = MessageValue
            }; //holy confusing code batman!

            messages.Add(message);
        }

        public override double GetLastHeartBeat()
        {
            return CommonCommunicator.UnixTime() - lastHeartbeat;
        }
    }
}
