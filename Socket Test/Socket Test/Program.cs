using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace Socket_Test
{
    class Program
    {
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
            
            //Lets connect to SMCS!
            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), smcsPort);
            client.Connect(serverEndPoint);

            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes("{'Type':4, 'CommandValue':'4'}");

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();

            Console.ReadLine();
        }
    }
}
