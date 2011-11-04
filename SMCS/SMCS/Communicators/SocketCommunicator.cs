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
            TcpClient client = (TcpClient)objClient;
            NetworkStream clientStream = client.GetStream();

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
                Console.WriteLine(message);
                if (message == "RepeatSteamReply")
                {
                    while (Program.steamConnectionReply == "") { } //Block it until it has a proper value!
                    SendRawMessage(client, Program.steamConnectionReply);
                    break;
                }

                Program.ProcessCommand(message);
            }

            client.Close();
        }

        void SendRawMessage(TcpClient Client, string Message)
        {
            byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(Message);
            Client.GetStream().Write(sendBytes, 0, sendBytes.Length);
        }
    }
}
