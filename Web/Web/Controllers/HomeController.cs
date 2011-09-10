using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Web.Models;
using Newtonsoft.Json;
using System.Configuration;
using System.Net.Sockets;
//using System.Diagnostics;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (ConfigurationManager.AppSettings["LoginEnabled"] == "Yes")
            {
                return View();
            }
            else
            {
                return Content("Login Disabled");
            }
        }

        public ActionResult FAQ()
        {
            return View();
        }
        
        public ActionResult Login(string userName, string passWord, string steamGuardAccessKey, string AndroidVersion)
        {
            if (ConfigurationManager.AppSettings["LoginEnabled"] != "Yes")
                return Content("LoginDisabled");
            if (AndroidVersion != null && ConfigurationManager.AppSettings["CheckMobileVersion"] == "Yes")
            {
                if (AndroidVersion != ConfigurationManager.AppSettings["AndroidVersion"])
                    return Content("Update");
            }

            TcpClient client = null;
            NetworkStream ns = null;
            try
            {
                client = new TcpClient("localhost", 8165);
                ns = client.GetStream();
            }
            catch
            {
                return Content("pocketSteamOffline");
            }

            DatabaseEntities db = new DatabaseEntities();
            string sessionToken = Guid.NewGuid().ToString().Replace('-', 'a');
            string passKey = Guid.NewGuid().ToString().Replace('-', 'd');

            string data = sessionToken + "\n" +
                          userName + "\n" +
                          passWord + "\n" +
                          steamGuardAccessKey;

            byte[] dataBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(data);
            ns.Write(dataBytes, 0, dataBytes.Length);

            bool waitingForReply = true;
            string CSMCSReply = "\n";
            DateTime SMCSTimeout = DateTime.Now;

            while (waitingForReply)
            {
                if ((DateTime.Now - SMCSTimeout).TotalSeconds > 10)
                {
                    waitingForReply = false;
                    return Content("pocketSteamOffline");
                }

                if (client.Available > 0)
                {
                    byte[] readBytes = new byte[client.Available];
                    ns.Read(readBytes, 0, readBytes.Length);
                    CSMCSReply = System.Text.ASCIIEncoding.ASCII.GetString(readBytes);
                    waitingForReply = false;
                }
            }

            string[] CSMCSReplyArray = System.Text.RegularExpressions.Regex.Split(CSMCSReply, "\n");
            int SMCSPort = -1;

            if (CSMCSReplyArray[0] == "Port")
            {
                SMCSPort = Int32.Parse(CSMCSReplyArray[1]);

                db.Sessions.AddObject(new Session
                {
                    SessionToken = sessionToken,
                    PassKey = passKey,
                    DateCreated = DateTime.Now,
                    LastHeartbeat = DateTime.Now,
                    IPAddress = Request.ServerVariables["REMOTE_ADDR"],
                    Status = 1,
                    SMCSPort = SMCSPort
                });
                db.SaveChanges();
            }
            else
            {
                waitingForReply = false;
                return Content("pocketSteamOffline");
            }
            client.Close();
            ns = null;

            //Connect to the SMCS Client
            Thread.Sleep(600); //Wait a bit for SMCS to start up
            try
            {
                client = new TcpClient("localhost", SMCSPort);
                ns = client.GetStream();
            }
            catch
            {
                return Content("pocketSteamOffline");
            }

            waitingForReply = true;
            SMCSTimeout = DateTime.Now;
            string SMCSReply = "\n";

            byte[] queryByte = { 0x00, 0x00 };
            ns.Write(queryByte, 0, queryByte.Length);

            while (waitingForReply)
            {
                if ((DateTime.Now - SMCSTimeout).TotalSeconds > 40)
                {
                    waitingForReply = false;
                    return Content("pocketSteamOffline");
                }

                if (client.Available > 0)
                {
                    byte[] readBytes = new byte[client.Available];
                    ns.Read(readBytes, 0, readBytes.Length);
                    SMCSReply = System.Text.ASCIIEncoding.ASCII.GetString(readBytes);
                    waitingForReply = false;
                }
            }

            if (SMCSReply == "Success")
            {
                return Content("Success:" + sessionToken + ":" + passKey);
            }
            else
            {
                return Content(SMCSReply);
            }
        }

        public ActionResult Display(string SessionToken)
        {
            DatabaseEntities db = new DatabaseEntities();
            List<Session> sessions = db.Sessions.Where(d => d.SessionToken == SessionToken).ToList();

            if (sessions.Count() == 1)
            {
                Session session = sessions[0];
                if (Request.Cookies["passkey"] != null)
                {
                    if (session.IPAddress == Request.ServerVariables["REMOTE_ADDR"] && session.PassKey == Request.Cookies["passkey"].Value)
                    {
                        return View(session);
                    }
                    else
                    {
                        return Content("Unautherised session access");
                    }
                }
                else
                {
                    return Content("Unautherised session access");
                }
            }
            else
            {
                return Content("Your session has expired");
            }
        }

        public ActionResult AJAXCommand(string SessionToken, int Type, string Command)
        {
            DatabaseEntities db = new DatabaseEntities();
            List<Session> sessions = db.Sessions.Where(d => d.SessionToken == SessionToken).ToList();

            if (sessions.Count() == 1)
            {
                Session session = sessions[0];
                if (Request.Cookies["passkey"] != null)
                {
                    if (session.IPAddress == Request.ServerVariables["REMOTE_ADDR"] && session.PassKey == Request.Cookies["passkey"].Value)
                    {
                        string commandSerialised = "";
                        if (Type == 1)
                        {
                            Command command = new Command
                            {
                                Type = 1,
                            };

                            commandSerialised = JsonConvert.SerializeObject(command);
                        }
                        else if (Type == 2 || Type == 3)
                        {
                            FriendMessageSend friendMessage = new FriendMessageSend
                            {
                                To = Request.Form["messageTo"],
                                Message = Request.Form["messageText"]
                            };
                            string messageJson = JsonConvert.SerializeObject(friendMessage);
                            Command command = new Command
                            {
                                Type = Type,
                                CommandValue = messageJson,
                            };

                            commandSerialised = JsonConvert.SerializeObject(command);
                        }
                        else
                        {
                            return Content("Unknown");
                        }

                        TcpClient client;
                        NetworkStream ns;
                        try
                        {
                            client = new TcpClient("localhost", session.SMCSPort);
                            ns = client.GetStream();
                        }
                        catch
                        {
                            return Content("ErrorNoSend");
                        }

                        byte[] writeBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(commandSerialised);
                        ns.Write(writeBytes, 0, writeBytes.Length);
                        client.Close();
                        return Content("OK");
                    }
                    else
                    {
                        return Content("Unautherised session access");
                    }
                }
                else
                {
                    return Content("Unautherised session access");
                }
            }
            else
            {
                return Content("No such session");
            }
        }

        public ActionResult AJAXReply(string SessionToken)
        {
            DatabaseEntities db = new DatabaseEntities();
            List<Session> sessions = db.Sessions.Where(d => d.SessionToken == SessionToken).ToList();

            if (sessions.Count() == 1)
            {
                Session session = sessions[0];
                if (Request.Cookies["passkey"] != null)
                {
                    if (session.IPAddress == Request.ServerVariables["REMOTE_ADDR"] && session.PassKey == Request.Cookies["passkey"].Value)
                    {
                        session.LastHeartbeat = DateTime.Now;
                        List<Message> messages = db.Messages.Where(d => d.SessionToken == session.SessionToken).ToList();

                        JsonReturn returnObject = new JsonReturn
                        {
                            Status = session.Status,
                            Messages = messages
                        };

                        foreach (Message message in messages) //Delete all old records so we don't recieve the same one twice!
                            db.Messages.DeleteObject(message);
                        db.SaveChanges();

                        string jsonReply = JsonConvert.SerializeObject(returnObject);
                        return Content(jsonReply);
                    }
                    else
                    {
                        return Content("Unautherised session access!!!!!!");
                    }
                }
                else
                {
                    return Content("Unautherised session access!!!!!!");
                }
            }
            else
            {
                return Content("No such session");
            }
        }
    }
}
