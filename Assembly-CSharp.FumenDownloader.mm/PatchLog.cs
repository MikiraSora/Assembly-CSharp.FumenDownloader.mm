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
        private const string DefaultFilePath = @"D:\dpFumenDownload.log";
        private static string filePath = DefaultFilePath;
        private static readonly object locker = new object();

        public static void SetFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            lock (locker)
            {
                try
                {
                    filePath = path;
                    var dir = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);
                    File.Delete(filePath);
                }
                catch
                {
                }
            }
        }

        public static void WriteLine(string msg)
        {
            lock (locker)
            {
                try
                {
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    File.AppendAllText(filePath, $"{DateTime.Now:HH:mm:ss.ff}[{threadId}]{msg}{Environment.NewLine}");
                }
                catch
                {
                }
            }
        }
    }
}
