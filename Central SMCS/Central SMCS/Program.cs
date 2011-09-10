using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace Central_SMCS
{
    class Program
    {
        static TcpListener server;
        static bool serverOnline = true;
        static Thread connectionThread = null;
        static byte byteNumber = 0;

        static void Main(string[] args)
        {
            Program program = new Program();

            Console.Write("Starting connection thread...");
            connectionThread = new Thread(new ThreadStart(program.StartServer));
            connectionThread.Name = "Connection thread";
            connectionThread.Start();
            Console.WriteLine("Done");


            while (serverOnline)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                {
                    StopServer();
                }
            }
        }

        static void StopServer()
        {
            connectionThread.Abort();
            serverOnline = false;
            server.Stop();
        }

        void StartServer()
        {
            Console.Write("Starting socket...");
            try
            {
                server = new TcpListener(IPAddress.Any, 8165);
                server.Start();
            }
            catch
            {
                Console.WriteLine("FAILED");
                StopServer();
                return;
            }
            Console.WriteLine("Done");

            while (serverOnline)
            {
                TcpClient newClient = server.AcceptTcpClient();

                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), newClient);
            }
        }

        void ProcessClient(object clientObject)
        {
            TcpClient client = (TcpClient)clientObject;

            byte[] bytes = new byte[1024];
            StringBuilder clientData = new StringBuilder();

            byteNumber += 1;
            int portNumber = 0;

            if (byteNumber < 9)
                portNumber = Int32.Parse("500" + byteNumber);
            else if (byteNumber < 99)
                portNumber = Int32.Parse("50" + byteNumber);
            else if (byteNumber > 99)
                portNumber = Int32.Parse("5" + byteNumber);

            byte[] writeBytes = ASCIIEncoding.ASCII.GetBytes("Port\n" + portNumber);
            client.GetStream().Write(writeBytes, 0, writeBytes.Length);

            using (NetworkStream stream = client.GetStream())
            {
                stream.ReadTimeout = 60000;

                int bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(bytes, 0, bytes.Length);

                    if (bytesRead > 0)
                    {
                        clientData.Append(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                        stream.ReadTimeout = 10000;
                    }
                }
                catch {}
            }

            string[] dataArray = Regex.Split(clientData.ToString(), "\n");

            if (dataArray.Count() != 4)
                return;

            try
            {
                string sessionToken = dataArray[0];
                string userName = dataArray[1];
                string passWord = dataArray[2];
                string authCode = dataArray[3];

                Process process = new Process();
                process.StartInfo.FileName = ConfigurationManager.AppSettings["SMCSLocation"];

                if (authCode == "")
                    process.StartInfo.Arguments = String.Format("-username {0} -password {1} -sessionToken {2} -port {3}",
                            userName,
                            passWord,
                            sessionToken,
                            portNumber
                        );
                else
                    process.StartInfo.Arguments = String.Format("-username {0} -password {1} -sessionToken {2} -authcode {3} -port{4}",
                            userName,
                            passWord,
                            sessionToken,
                            authCode,
                            portNumber
                        );
                
                process.Start();
                process.Close();

                client.Close();

                Console.WriteLine(DateTime.Now + " | Started: " + userName);
            } catch {
                Console.WriteLine("OMG ERROR");
            }
        }
    }
}
