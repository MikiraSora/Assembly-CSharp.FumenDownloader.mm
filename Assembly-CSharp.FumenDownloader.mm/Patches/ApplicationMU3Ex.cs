using DpPatches.FumenDownloader.Kernel;
using MonoMod;
using MU3.App;
using MU3.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace DpPatches.FumenDownloader.Patches
{
    [MonoModPatch("global::MU3.App.ApplicationMU3")]
    internal class ApplicationMU3Ex : ApplicationMU3
    {
        private volatile static bool fumenLoadDone;

        static ApplicationMU3Ex()
        {
            //register event to make my life better :D
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Application.logMessageReceived += Application_logMessageReceived;
            Application.logMessageReceivedThreaded += Application_logMessageReceived;

            PatchLog.WriteLine("");
            PatchLog.WriteLine($"Log init date : " + DateTime.Now.ToLongDateString());
            PatchLog.WriteLine($"Log init time : {DateTime.Now.ToLongTimeString()}");
            PatchLog.WriteLine("");

            Setting.Init();

            new Thread(OnFumenLoadThread)
            {
                IsBackground = true,
                Name = "DpPatches.OnFumenLoadThread"
            }.Start();
        }

        private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                PatchLog.WriteLine($"Application_logMessageReceived() catch exception:");
                PatchLog.WriteLine($"Application_logMessageReceived() condition: {condition}");
                PatchLog.WriteLine($"Application_logMessageReceived() stackTrace: \n{stackTrace}");
                PatchLog.WriteLine("");
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            PatchLog.WriteLine($"CurrentDomain_UnhandledException() catch unhandle exception:");
            PatchLog.WriteLine($"CurrentDomain_UnhandledException() e.IsTerminating: {e.IsTerminating}");
            if (e?.ExceptionObject is Exception ex)
            {
                ExceptionDumpper.Dump(ex);
            }
            else
            {
                PatchLog.WriteLine($"CurrentDomain_UnhandledException() e.ExceptionObject isn't exception: {e?.ExceptionObject}");
            }
            PatchLog.WriteLine("");
        }

        private extern void orig_Execute_LoadGameData();

        private void Execute_LoadGameData()
        {
            if (!fumenLoadDone)
                return;

            orig_Execute_LoadGameData();
        }

        private static void OnFumenLoadThread()
        {
            if (Setting.Enable)
            {
                try
                {
                    PatchLog.WriteLine($"Begin.");
                    OnFumenLoad();
                    PatchLog.WriteLine($"Fumen initializeion done");
                }
                catch (Exception e)
                {
                    PatchLog.WriteLine($"OnFumenLoadThread() throw exception: {e?.Message}");
                    ExceptionDumpper.Dump(e);
                }
            }
            fumenLoadDone = true;
        }

        private static void OnFumenLoad()
        {
            Singleton<FumenDownloaderManager>.instance.Initalize();
        }
    }
}
