using MU3.Collab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DpPatches.FumenDownloader
{
    public static class PatchLog
    {
        public const string FilePath = "dpFumenDownload.log";
        private static object locker = new object();

        static PatchLog()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch { }
        }

        public static void WriteLine(string msg)
        {
            lock (locker)
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                File.AppendAllText(FilePath, $"{DateTime.Now:HH:mm:ss.ff}[{threadId}]{msg}{Environment.NewLine}");
            }
        }
    }
}
