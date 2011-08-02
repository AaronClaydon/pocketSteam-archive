﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;
using SteamKit2;

namespace SMCS
{
    struct AvatarDownloadDetails
    {
        public bool Success;
        public string Filename;
        public Exception Exception;
    }

    class AvatarData
    {
        public string hashstr;
        public string localFile;
        public Action<AvatarDownloadDetails> callback;
    }

    static class CDNCache
    {

        const string AvatarRoot = "http://media.steampowered.com/steamcommunity/public/images/avatars/";

        const string AvatarFull = "{0}/{1}_full.jpg";
        const string AvatarMedium = "{0}/{1}_medium.jpg";
        const string AvatarSmall = "{0}/{1}.jpg";


        static Thread cdnThread;

        static readonly object lockObj = new object();
        static Queue<AvatarData> jobQueue;

        static Dictionary<SteamID, byte[]> avatarMap;

        public static void Initialize()
        {
            avatarMap = new Dictionary<SteamID, byte[]>();
            jobQueue = new Queue<AvatarData>();

            cdnThread = new Thread( JobFunc );
            cdnThread.Start();
        }

        public static void Shutdown()
        {
            AddJob( null );
            cdnThread.Join();
        }

        public static byte[] GetAvatarHash( SteamID steamId )
        {
            if ( avatarMap.ContainsKey( steamId ) )
                return avatarMap[ steamId ];

            return null;
        }

        public static string GetAvatarUrl(byte[] AvatarHash)
        {
            string hashStr = BitConverter.ToString( AvatarHash ).Replace( "-", "" ).ToLower();
            string hashPrefix = hashStr.Substring( 0, 2 );
            string downloadUri = string.Format(AvatarRoot + AvatarSmall, hashPrefix, hashStr);

            return downloadUri;
        }

        public static void DownloadAvatar( SteamID steamId, byte[] avatarHash, Action<AvatarDownloadDetails> callBack )
        {
            if(avatarMap.ContainsKey(steamId))
                avatarMap[steamId] = avatarHash;
            else
                avatarMap.Add(steamId, avatarHash);

            string hashStr = BitConverter.ToString( avatarHash ).Replace( "-", "" ).ToLower();
            string hashPrefix = hashStr.Substring( 0, 2 );

            string localPath = Path.Combine( Application.StartupPath, "avatars" );

            if ( !Directory.Exists( localPath ) )
            {
                try
                {
                    Directory.CreateDirectory( localPath ); // try making the cache directory
                    DebugLog.WriteLine( "CDNCache", "Creating cache directory for avatars." );
                }
                catch ( Exception ex )
                {
                    DebugLog.WriteLine( "CDNCache", "Unable to create cache directory.\n{0}", ex.ToString() );
                    callBack( new AvatarDownloadDetails() { Success = false, Exception = ex } );
                    return;
                }
            }

            string localFile = Path.Combine( localPath, hashStr + ".jpg" );
            if ( File.Exists( localFile ) )
            {
                callBack( new AvatarDownloadDetails() { Success = true, Filename = localFile } );
                return;
            }

            AvatarData ad = new AvatarData
            {
                localFile = localFile,
                callback = callBack,
                hashstr = hashStr,
            };

            AddJob( ad );
        }

        static void AddJob( AvatarData data )
        {
            lock ( lockObj )
            {
                jobQueue.Enqueue( data );
                Monitor.Pulse( lockObj );
            }
        }

        static void JobFunc()
        {
            while ( true )
            {

                AvatarData data;

                lock ( lockObj )
                {
                    while ( jobQueue.Count == 0 )
                        Monitor.Wait( lockObj );

                    data = jobQueue.Dequeue();
                }

                if ( data == null )
                    return; // exit signal

                string hashStr = data.hashstr;
                string hashPrefix = hashStr.Substring( 0, 2 );
                string localPath = Path.Combine( Application.StartupPath, "avatars" );
                var callBack = data.callback;


                string downloadUri = string.Format( AvatarRoot + AvatarSmall, hashPrefix, hashStr );
                string localFile = Path.Combine( localPath, hashStr + ".jpg" );

                using ( WebClient client = new WebClient() )
                {
                    try
                    {
                        client.DownloadFile( downloadUri, localFile );
                    }
                    catch ( Exception ex )
                    {
                        DebugLog.WriteLine( "CDNCache", "Unable to download avatar.\n{0}", ex.ToString() );
                        callBack( new AvatarDownloadDetails() { Success = false, Exception = ex } );
                        continue;
                    }
                }

                callBack( new AvatarDownloadDetails() { Success = true, Filename = localFile } );
            }
        }
    }
}
