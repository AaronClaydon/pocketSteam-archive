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
        static Thread connectionThread;
        static byte byteNumber = 0;

        static DateTime CSMCSStarted = DateTime.Now;
        static int SMCSStarted = 0;

        static void Main(string[] args)
        {
            ChangeTitle();
            Program program = new Program();

            connectionThread = new Thread(new ThreadStart(program.StartServer));
            connectionThread.Name = "Connection thread";
            connectionThread.Start();

            while (serverOnline)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key == ConsoleKey.Escape)
                {
                    StopServer();
                }
            }
        }

        static void ChangeTitle()
        {
            Console.Title = String.Format("Central pocketSteam Server - Started: {0}, served {1} clients", CSMCSStarted, SMCSStarted);
        }

        static void StopServer()
        {
            connectionThread.Abort();
            serverOnline = false;
            server.Stop();
        }

        void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, Int32.Parse(GlobalConfig.Get()["CSMCS-Port"]));
                server.Start();
            }
            catch
            {
                Console.WriteLine("CSMCS SOCKET INITIATE FAILED");
                StopServer();
                return;
            }

            while (serverOnline)
            {
                TcpClient newClient = server.AcceptTcpClient();

                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), newClient);
            }
        }

        void ProcessClient(object clientObject)
        {
            Dictionary<String, String> globalConfig = GlobalConfig.Get();
            TcpClient client = (TcpClient)clientObject;

            byte[] bytes = new byte[1024];
            StringBuilder clientData = new StringBuilder();

            byteNumber += 1;
            int portNumber = Int32.Parse(byteNumber.ToString(globalConfig["SMCS-Base-Port"]));

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
                process.StartInfo.FileName = globalConfig["SMCS-Location"];

                if (authCode == "")
                    process.StartInfo.Arguments += String.Format(" -username {0} -password {1} -sessionToken {2} -port {3}",
                            userName,
                            passWord,
                            sessionToken,
                            portNumber
                        );
                else
                    process.StartInfo.Arguments += String.Format(" -username {0} -password {1} -sessionToken {2} -authcode {3} -port {4}",
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
                SMCSStarted++;
                ChangeTitle();
            } catch(Exception ex) {
                Console.WriteLine("Could not start SMCS - " + ex.Message);
            }
        }
    }
}
