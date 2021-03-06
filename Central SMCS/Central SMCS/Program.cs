﻿using System;
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
        static Dictionary<String, String> globalConfig = GlobalConfig.Get();

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
            catch (Exception ex)
            {
                Console.WriteLine("CSMCS SOCKET INITIATE FAILED: " + ex.Message);
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
            TcpClient client = (TcpClient)clientObject;

            byte[] bytes = new byte[1024];
            StringBuilder clientData = new StringBuilder();

            byteNumber += 1;
            int portNumber = Int32.Parse(byteNumber.ToString(globalConfig["SMCS-Base-Port"]));

            byte[] writeBytes = Encoding.ASCII.GetBytes("Port\n" + portNumber);
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

            if (dataArray.Count() != 5)
            {
                Console.WriteLine("Invalid data length");
                return;
            }

            try
            {
                string sessionToken = dataArray[0];
                string userName = dataArray[1];
                string passWord = dataArray[2];
                string platform = dataArray[3];
                string authCode = dataArray[4];

                Process process = new Process();
                process.StartInfo.FileName = globalConfig["SMCS-Location"];

                if (authCode == "")
                    process.StartInfo.Arguments = String.Format("-platform {0} -username {1} -password {2} -sessionToken {3} -port {4}",
                            platform,
                            userName,
                            passWord,
                            sessionToken,
                            portNumber
                        );
                else
                    process.StartInfo.Arguments = String.Format("-platform {0} -username {1} -password {2} -sessionToken {3} -authcode {4} -port {5}",
                            platform,
                            userName,
                            passWord,
                            sessionToken,
                            authCode,
                            portNumber
                        );
                
                process.Start();
                process.Close();

                client.Close();

                Console.WriteLine(DateTime.Now + " | Started: [" + platform + "] " + userName);
                SMCSStarted++;
                ChangeTitle();
            } catch(Exception ex) {
                Console.WriteLine("Could not start SMCS - " + ex.Message);
            }
        }
    }
}
