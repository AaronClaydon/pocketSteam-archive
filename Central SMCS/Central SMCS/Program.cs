using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Configuration;

namespace Central_SMCS
{
    class Program
    {
        static DatabaseEntities db = new DatabaseEntities();

        static void Main(string[] args)
        {
            CreateNewSessions();
        }

        static void CreateNewSessions()
        {
            while (true)
            {
                try
                {
                    List<NewSession> newSessions = db.NewSessions.ToList();
                    db.Refresh(System.Data.Objects.RefreshMode.ClientWins, newSessions);

                    foreach (NewSession session in newSessions)
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = ConfigurationManager.AppSettings["SMCSLocation"];

                        if (session.SteamGuardAuth == "")
                            process.StartInfo.Arguments = String.Format("-username {0} -password {1} -sessionToken {2}",
                                    session.Username,
                                    session.Password,
                                    session.SessionToken
                                );
                        else
                            process.StartInfo.Arguments = String.Format("-username {0} -password {1} -sessionToken {2} -authcode {3}",
                                    session.Username,
                                    session.Password,
                                    session.SessionToken,
                                    session.SteamGuardAuth
                                );

                        process.Start();
                        process.Close();

                        Console.WriteLine(DateTime.Now + " | Started: " + session.Username);

                        db.NewSessions.DeleteObject(session);
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + " | Database error: (" + ex.Message + ")" + ex.InnerException);
                }
                Thread.Sleep(1200);
            }
        }
    }
}
