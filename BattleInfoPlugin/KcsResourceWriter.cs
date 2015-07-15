using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Nekoxy;
using Grabacr07.KanColleWrapper;

namespace BattleInfoPlugin
{
    class KcsResourceWriter
    {
        public KcsResourceWriter()
        {
            var resources = KanColleClient.Current.Proxy.SessionSource
                .Where(s => s.Request.PathAndQuery.StartsWith("/kcs/resources/swf/map"));

            resources.Subscribe(s => s.SaveResponseBody(s.GetSaveFilePath()));
        }
    }

    static class KcsResourceWriterExtensions
    {
        public static string GetSaveFilePath(this Session session)
        {
            return Properties.Settings.Default.CacheDirPath
                   + session.Request.PathAndQuery.Split('?').First();
        }

        private static readonly object lockObj = new object();

        public static void SaveResponseBody(this Session session, string filePath)
        {
            lock (lockObj)
            {
                File.WriteAllBytes(filePath, session.Response.Body);
            }
        }
    }
}
