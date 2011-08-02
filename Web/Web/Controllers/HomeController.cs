using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SteamKit2;
using System.Threading;
using Web.Models;
using Newtonsoft.Json;
//using System.Diagnostics;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            /*
            int SMCSLimit = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["SMCSLimit"]);
            int SMCSCount = Process.GetProcesses().ToList().Where(d => d.ProcessName == "SMCS.exe").Count();
            if (SMCSCount >= SMCSLimit)
                return View("TooManySessions");
            */
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }
        
        public ActionResult Login(string userName, string passWord, string steamGuardAccessKey)
        {
            ClientTGT clientTgt;
            byte[] serverTgt;
            AuthBlob accRecord;

            try
            {
                Steam2.Initialize(userName, passWord, out clientTgt, out serverTgt, out accRecord);
            }
            catch
            {
                return Content("Invalid");
            }

            try
            {
                if (steamGuardAccessKey != "" && steamGuardAccessKey != null)
                    Steam3.AuthCode = steamGuardAccessKey;

                Steam3.UserName = userName;
                Steam3.Password = passWord;

                Steam3.ClientTGT = clientTgt;
                Steam3.ServerTGT = serverTgt;
                Steam3.AccountRecord = accRecord;

                Steam3.AlternateLogon = false; //true = Uses PS3 logon

                Steam3.Initialize(true);

                Steam3.Connect();
            }
            catch (Steam3Exception ex)
            {
                Steam3.Shutdown();
                return Content("UnknownConnectFail " + ex.InnerException);
            }

            SteamHandler handler = new SteamHandler();
            string ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            handler.Data(userName, passWord, steamGuardAccessKey, ipAddress);
            Steam3.AddHandler(handler);

            while (handler.Waiting)
            {
                Steam3.Update();
                Thread.Sleep(1);
            }

            Steam3.Shutdown();
            
            return Content(handler.Return);
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
                        if (Type == 1)
                        {
                            Command command = new Command
                            {
                                SessionToken = SessionToken,
                                Type = 1,
                                DateCreated = DateTime.Now
                            };

                            db.Commands.AddObject(command);
                            db.SaveChanges();

                            return Content("OK");
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
                                SessionToken = SessionToken,
                                Type = Type,
                                CommandValue = messageJson,
                                DateCreated = DateTime.Now
                            };

                            db.Commands.AddObject(command);
                            db.SaveChanges();

                            return Content("OK");
                        }
                        else
                        {
                            return Content("Unknown");
                        }
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
