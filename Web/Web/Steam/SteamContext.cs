﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Net;

namespace Web
{
    class Steam2Exception : Exception
    {
        public Steam2Exception( string msg )
            : base( msg )
        {
        }
    }

    class Steam3Exception : Exception
    {
        public Steam3Exception( string msg, Exception inner )
            : base( msg, inner )
        {
        }
    }

    interface ICallbackHandler
    {
        void HandleCallback( CallbackMsg msg );
    }

    static class Steam3
    {
        public static SteamClient SteamClient { get; private set; }

        public static SteamFriends SteamFriends { get; private set; }
        public static SteamUser SteamUser { get; private set; }

        static List<ICallbackHandler> callbackHandlers;

        public static string UserName { get; set; }
        public static string Password { get; set; }

        public static string AuthCode { get; set; }

        public static bool AlternateLogon { get; set; }


        static Steam3()
        {
            callbackHandlers = new List<ICallbackHandler>();
        }


        public static void Initialize( bool useTcp )
        {
            SteamClient = new SteamClient( useTcp ? CMClient.ConnectionType.Tcp : CMClient.ConnectionType.Udp );

            SteamFriends = SteamClient.GetHandler<SteamFriends>();
            SteamUser = SteamClient.GetHandler<SteamUser>();
        }

        public static void Connect()
        {
            try
            {
                SteamClient.Connect();
            }
            catch ( Exception ex )
            {
                throw new Steam3Exception( "Unable to connect to CM server.", ex );
            }
        }

        public static void Shutdown()
        {
            SteamUser.LogOff();
            SteamClient.Disconnect();
        }

        public static void AddHandler( ICallbackHandler handler )
        {
            callbackHandlers.Add( handler );
        }
        public static void RemoveHandler( ICallbackHandler handler )
        {
            callbackHandlers.Remove( handler );
        }

        public static void Update()
        {
            CallbackMsg msg = SteamClient.GetCallback();

            if ( msg == null )
                return;

            SteamClient.FreeLastCallback();

            if ( msg.IsType<SteamClient.ConnectCallback>() )
            {
                SteamUser.LogOn( new SteamUser.LogOnDetails()
                    {
                        Username = Steam3.UserName,
                        Password = Steam3.Password,

                        AuthCode = Steam3.AuthCode,

                        AccountInstance = ( Steam3.AlternateLogon ? 2u : 1u ),
                    } );
            }

            List<ICallbackHandler> tempHandlers = new List<ICallbackHandler>( callbackHandlers );

            // push it along to anyone who wants to handle this
            foreach ( ICallbackHandler handler in tempHandlers )
            {
                handler.HandleCallback( msg );
            }
        }
    }
}
