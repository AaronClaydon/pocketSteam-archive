using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace Socket_Test
{
    class Program
    {
        static NetworkStream clientStream;

        static void Main(string[] args)
        {
            //Authenticate using the website
            Console.WriteLine("Authing session with web...");

            string formUrl = "http://localhost/pocketsteam/index.php/main/login";
            string formParams = string.Format("userName={0}&passWord={1}&platform={2}", "azzytest", "testing123", "SteamFriendsAndroid");
            WebRequest req = WebRequest.Create(formUrl);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(formParams);
            req.ContentLength = bytes.Length;
            using (Stream os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }
            byte[] responseBytes = new byte[512];
            WebResponse resp = req.GetResponse();
            int bytesRead;
            using (Stream rs = resp.GetResponseStream())
            {
                bytesRead = rs.Read(responseBytes, 0, responseBytes.Length);
            }
            string responseString = ASCIIEncoding.ASCII.GetString(responseBytes, 0, bytesRead);
            string[] responseArray = responseString.Split(':');
            int smcsPort = Int32.Parse(responseArray[3]);
            Console.WriteLine("Auth response: " + responseString);
            Console.WriteLine("SMCS Port: " + smcsPort);

            //Lets connect to SMCS!
            Console.WriteLine("Starting SMCS Connection");
            Thread connectionThread = new Thread(new ParameterizedThreadStart(HandleConnection));
            connectionThread.Start(smcsPort);

            Thread heartBeatThread = new Thread(new ThreadStart(delegate()
            {
                while (true)
                {
                    SendMessage("HEART");
                    Thread.Sleep(5000);
                }
            }));
            heartBeatThread.Start();

            Console.ReadLine();
        }

        static void HandleConnection(object port)
        {
            TcpClient  client = new TcpClient();
            client.Connect("127.0.0.1", (int)port);

            clientStream = client.GetStream();
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
                catch(Exception ex)
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                string rawMessage = ASCIIEncoding.ASCII.GetString(bytes, 0, bytesRead);

                SocketMessage message;
                try
                {
                    message = JsonConvert.DeserializeObject<SocketMessage>(rawMessage);

                    if (message.Type == 1)
                    {
                        SteamUserData user = JsonConvert.DeserializeObject<SteamUserData>(message.MessageValue);
                        Console.WriteLine("Your data: {0} - {1}", user.N, user.St);
                        Console.WriteLine();
                    }
                    else if (message.Type == 2 || message.Type == 3)
                    {
                        FriendMessageData steamMessage = JsonConvert.DeserializeObject<FriendMessageData>(message.MessageValue);

                        if (message.Type == 2)
                            Console.WriteLine("{0} says {1}", steamMessage.N, steamMessage.M);
                        else
                            Console.WriteLine("{0} {1}", steamMessage.N, steamMessage.M);

                        Console.WriteLine();
                    }
                    else if (message.Type == 4)
                    {
                        Console.WriteLine("New friends list");
                        List<SteamUserData> friends = JsonConvert.DeserializeObject<List<SteamUserData>>(message.MessageValue);

                        foreach (SteamUserData friend in friends)
                        {
                            Console.WriteLine("   {0} - {1}", friend.N, friend.St);

                            FriendMessageSend friendMessage = new FriendMessageSend
                            {
                                To = friend.SID,
                                Message = "Hello there!"
                            };
                            Command command = new Command
                            {
                                Type = 2,
                                CommandValue = JsonConvert.SerializeObject(friendMessage)
                            };

                            string messageJson = JsonConvert.SerializeObject(command);
                            //SendMessage(messageJson);
                        }
                        Console.WriteLine();
                    }
                } catch {}
            }

            client.Close();
        }

        static void SendMessage(string Message)
        {
            if (clientStream != null)
            {
                byte[] bytes = ASCIIEncoding.ASCII.GetBytes(Message);
                clientStream.Write(bytes, 0, bytes.Length);
                clientStream.Flush();
            }
        }
    }
}