using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SteamKit2;
using System.Configuration;
using Web.Models;

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
                        NewSession newSession = new NewSession
                        {
                            SessionToken = sessionToken,
                            Username = userName,
                            Password = passWord,
                            SteamGuardAuth = authCode,
                            DateCreated = DateTime.Now
                        };
                        db.NewSessions.AddObject(newSession);

                        openedSMCS = true;
                    }
                    /*
                    if (!openedSMCS)
                    {
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        process.StartInfo.FileName = ConfigurationManager.AppSettings["SMCSLocation"];

                        if (authCode == "")
                            process.StartInfo.Arguments = String.Format("-username {0} -password {1} -sessionToken {2}",
                                    userName,
                                    passWord,
                                    sessionToken
                                );
                        else
                            process.StartInfo.Arguments = String.Format("-username {0} -password {1} -sessionToken {2} -authcode {3}",
                                    userName,
                                    passWord,
                                    sessionToken,
                                    authCode
                                );

                        openedSMCS = process.Start();
                        process.Close();
                    } */
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