using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SteamKit2;
using System.Configuration;
using Web.Models;
using System.Net.Sockets;

namespace Web
{
    public class SteamHandler : ICallbackHandler
    {
        public bool Waiting = true;
        public string Return { get; set; }

        string userName { get; set; }
        string passWord { get; set; }
        string authCode { get; set; }

        string ipAddress { get; set; }

        bool openedSMCS = false;

        public void Data(string userName, string passWord, string authCode, string ipAddress)
        {
            this.userName = userName;
            this.passWord = passWord;
            this.authCode = authCode;
            this.ipAddress = ipAddress;
        }

        public void HandleCallback(CallbackMsg msg)
        {
            if (msg.IsType<SteamUser.LogOnCallback>())
            {
                var logOnResp = (SteamUser.LogOnCallback)msg;

                if (logOnResp.Result == EResult.OK)
                {
                    TcpClient client = null;
                    NetworkStream ns = null;
                    try
                    {
                        client = new TcpClient("localhost", 8165);
                        ns = client.GetStream();
                    }
                    catch
                    {
                        openedSMCS = true;
                        Steam3.Shutdown();
                        Waiting = false;
                        Return = "pocketSteamOffline";
                        Steam3.RemoveHandler(this);
                        return;
                    }

                    DatabaseEntities db = new DatabaseEntities();

                    string sessionToken = Guid.NewGuid().ToString().Replace('-', 'a');
                    string passKey = Guid.NewGuid().ToString().Replace('-', 'd');

                    db.Sessions.AddObject(new Session { 
                        SessionToken = sessionToken,
                        PassKey = passKey,
                        DateCreated = DateTime.Now,
                        LastHeartbeat = DateTime.Now,
                        IPAddress = ipAddress,
                        Status = 1
                    });

                    Waiting = false;
                    Return = "Success:" + sessionToken + ":" + passKey;

                    if (!openedSMCS)
                    {
                        /*
                        NewSession newSession = new NewSession
                        {
                            SessionToken = sessionToken,
                            Username = userName,
                            Password = passWord,
                            SteamGuardAuth = authCode,
                            DateCreated = DateTime.Now
                        };
                        db.NewSessions.AddObject(newSession);
                        */
                        string data = sessionToken + "\n" +
                                      userName + "\n" +
                                      passWord + "\n" +
                                      authCode;

                        byte[] dataBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(data);
                        ns.Write(dataBytes, 0, dataBytes.Length);
                        ns.Close();
                        client.Close();

                        openedSMCS = true;
                    }
                    
                    db.SaveChanges();
                    Steam3.Shutdown();
                }
                else if (logOnResp.Result == EResult.AccountLogonDenied || logOnResp.Result == EResult.InvalidLoginAuthCode)
                {
                    Steam3.Shutdown();
                    Waiting = false;
                    Return = "SteamGuard";
                    Steam3.RemoveHandler(this);
                }
                else if (logOnResp.Result == EResult.AlreadyLoggedInElsewhere)
                {
                    Steam3.Shutdown();
                    Waiting = false;
                    Return = "LoggedInElsewhere";
                    Steam3.RemoveHandler(this);
                }
                else if (logOnResp.Result != EResult.OK)
                {
                    Steam3.Shutdown();
                    Waiting = false;
                    Return = "UnknownConnectFail " + logOnResp.Result;
                    Steam3.RemoveHandler(this);
                }
            }
        }
    }
}