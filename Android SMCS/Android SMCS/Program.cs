using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Android_SMCS
{
    class Program
    {
        static TcpListener listener;

        static void Main(string[] args)
        {
            listener = new TcpListener(IPAddress.Any, 8167);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                HandleComm(client);
                break;
            }

            Console.WriteLine("I died");
            Console.ReadKey();
        }

        static void HandleComm(TcpClient client)
        {
            Console.WriteLine("Got client");
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();

            byte[] message = new byte[4096];
            int bytesRead;

            Thread.Sleep(1000);

            Console.WriteLine("SENDING");
            clientStream.Write(encoder.GetBytes("Piss off!"), 0, encoder.GetBytes("Piss off!").Count());

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
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

                //message has successfully been received
                Console.WriteLine(encoder.GetString(message, 0, bytesRead));

                byte[] replyByte = encoder.GetBytes("Piss off!");
                clientStream.Write(replyByte, 0, replyByte.Count());
            }

            tcpClient.Close();
        }
    }
}
