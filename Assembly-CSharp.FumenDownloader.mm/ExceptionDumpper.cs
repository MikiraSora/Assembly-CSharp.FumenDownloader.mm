using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DpPatches.FumenDownloader
{
    internal static class ExceptionDumpper
    {
        public static void Dump(Exception ex, int spaceTab = 1)
        {
            if (ex == null)
                return;

            var prefix = "-" + string.Concat(Enumerable.Repeat(" ", spaceTab));
            void log(string msg) => PatchLog.WriteLine(prefix + msg);

            void dumpCur(Exception e)
            {
                log($"Message: {e.Message}");
                log($"Source: {e.Source}");
                log($"StackTrace:");
                foreach (var msg in e.StackTrace.Split('\n'))
                    log(msg.Trim());
                PatchLog.WriteLine("");
            }

            dumpCur(ex);

            if (ex.InnerException is Exception ex2)
            {
                log($"Dump inner exception:");
                Dump(ex2, spaceTab + 2);
            }
        }
    }

}
