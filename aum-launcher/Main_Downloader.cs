using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Octokit;
using Semver;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace aum_launcher
{
    public partial class Main
    {
        class DownloadWorker_TaskInfo
        {
            public WorkerProgress_ReleaseInfo Release;
            public bool IsLauncherUpdate;

            public string AUM_ReleaseFilePath = "";
            public string AUM_DebugFilePath = "";
            public string Launcher_FilePath = "";

            public bool Success = false;

            public DownloadWorker_TaskInfo(WorkerProgress_ReleaseInfo release, bool isLauncherUpdate)
            {
                Release = release;
                IsLauncherUpdate = isLauncherUpdate;
            }
        }
        // NOTE:
        // this class borrows githubClient from Main_Github
        // it's responsibilities are distinct enough that i decided to separate their logic, but the data needs to be shared

        bool IsDownloadingMenuUpdate = false;
        bool IsDownloadingLauncherUpdate = false;

        public bool IsDownloading { get { return IsDownloadingMenuUpdate || IsDownloadingLauncherUpdate; } }

        void DownloadAndInstallMenuFromRelease(WorkerProgress_ReleaseInfo release)
        {
            IsDownloadingMenuUpdate = true;
            while (DownloadWorker.IsBusy)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            DownloadWorker.RunWorkerAsync(new DownloadWorker_TaskInfo(release, false));
        }

        void DownloadAndInstallLauncherFromRelease(WorkerProgress_ReleaseInfo release)
        {
            IsDownloadingLauncherUpdate = true;
            if (File.Exists(Process.GetCurrentProcess().MainModule.FileName))
            {
                File.Move(Process.GetCurrentProcess().MainModule.FileName, Process.GetCurrentProcess().MainModule.FileName + "_old");
                Logger.Log.Write("Renamed launcher to '" + Process.GetCurrentProcess().MainModule.FileName + "_old' in preparation for update", Logger.ELogType.Info, rtxtLog);
            }
            else
            {
                Logger.Log.WriteError("Could not get existing launcher file, cannot update.", rtxtLog);
                return;
            }
            while (DownloadWorker.IsBusy)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            DownloadWorker.RunWorkerAsync(new DownloadWorker_TaskInfo(release, true));
        }

        void FinishInstallingNewMenuRelease(DownloadWorker_TaskInfo taskInfo)
        {
            // check if modDir + "Release\\" directory exists
            // delete all files within if it does, otherwise create the directory
            if (Directory.Exists(Main.MOD_RELEASE_FOLDER_NAME))
            {
                Logger.Log.Write("Clearing " + Main.MOD_RELEASE_FOLDER_NAME + " of existing files", Logger.ELogType.Info, rtxtLog);
                foreach (string filename in Directory.EnumerateFiles(Main.MOD_RELEASE_FOLDER_NAME))
                {
                    File.Delete(filename);
                }
            }
            else
            {
                Logger.Log.Write("Creating new " + Main.MOD_RELEASE_FOLDER_NAME + " directory", Logger.ELogType.Info, rtxtLog);
                Directory.CreateDirectory(Main.MOD_RELEASE_FOLDER_NAME);
            }
            // unzip taskInfo.releaseFilePath to modDir + "Release\\" directory
            ZipFile.ExtractToDirectory(taskInfo.AUM_ReleaseFilePath, Main.MOD_RELEASE_FOLDER_NAME);
            File.Delete(taskInfo.AUM_ReleaseFilePath);
            Logger.Log.Write("Extracted release assets to " + Main.MOD_RELEASE_FOLDER_NAME, Logger.ELogType.Info, rtxtLog);

            // check if modDir + "Debug\\" directory exists
            // delete all files within if it does, otherwise create the directory
            if (Directory.Exists(Main.MOD_DEBUG_FOLDER_NAME))
            {
                Logger.Log.Write("Clearing " + Main.MOD_DEBUG_FOLDER_NAME + " of existing files", Logger.ELogType.Info, rtxtLog);
                foreach (string filename in Directory.EnumerateFiles(Main.MOD_DEBUG_FOLDER_NAME))
                {
                    File.Delete(filename);
                }
            }
            else
            {
                Logger.Log.Write("Creating new " + Main.MOD_DEBUG_FOLDER_NAME + " directory", Logger.ELogType.Info, rtxtLog);
                Directory.CreateDirectory(Main.MOD_DEBUG_FOLDER_NAME);
            }
            // unzip taskInfo.debugFilePath to modDir + "Debug\\" directory
            ZipFile.ExtractToDirectory(taskInfo.AUM_DebugFilePath, Main.MOD_DEBUG_FOLDER_NAME);
            File.Delete(taskInfo.AUM_DebugFilePath);
            Logger.Log.Write("Extracted debug assets to " + Main.MOD_DEBUG_FOLDER_NAME, Logger.ELogType.Info, rtxtLog);
            Logger.Log.Write("Done installing " + taskInfo.Release.TagSemver.ToString(), Logger.ELogType.Notification, rtxtLog, true);
            IsDownloadingMenuUpdate = false;
            ActiveProfile.LocalTaggedSemVer = taskInfo.Release.TagSemver;
            if (taskInfo.Release.TagSemver.CompareByPrecedence(LatestMenuRelease.TagSemver) >= 0)
            {
                StatusLbl_AUM.ForeColor = System.Drawing.Color.LimeGreen;
                StatusLbl_AUM.Text = "AUM: OK";
            }
            else
            {
                StatusLbl_AUM.ForeColor = System.Drawing.Color.DarkOrange;
                StatusLbl_AUM.Text = "AUM: Update!";
            }
        }

        void FinishInstallingNewLauncherRelease(DownloadWorker_TaskInfo taskInfo)
        {
            Logger.Log.Write("Finished downloading launcher update, restarting now", Logger.ELogType.Notification, rtxtLog, true);
            // start decoupled process from taskInfo.launcherFilePath
            Process newLauncherProc = Process.Start(taskInfo.Launcher_FilePath);
            // exit current process
            System.Windows.Forms.Application.Exit();
        }

        private void DownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DownloadWorker_TaskInfo taskInfo = e.Argument as DownloadWorker_TaskInfo;

            try
            {
                if (taskInfo.IsLauncherUpdate)
                {
                    var taskDownload = githubClient.Connection.Get<byte[]>(new Uri(taskInfo.Release.LauncherExe_DownloadURL), new Dictionary<string, string>(), "application/octet-stream");
                    byte[] downloadBytes = taskDownload.Result.Body;
                    taskInfo.Launcher_FilePath = Main.LAUNCHER_NAME + "-" + taskInfo.Release.LauncherSemver.ToString() + ".exe";
                    File.WriteAllBytes(taskInfo.Launcher_FilePath, downloadBytes);
                }
                else
                {
                    var taskReleaseDownload = githubClient.Connection.Get<byte[]>(new Uri(taskInfo.Release.ReleaseZip_DownloadURL), new Dictionary<string, string>(), "application/octet-stream");
                    byte[] releaseDownloadBytes = taskReleaseDownload.Result.Body;
                    taskInfo.AUM_ReleaseFilePath = Main.MOD_RELATIVE_PATH + "Release.zip";
                    File.WriteAllBytes(taskInfo.AUM_ReleaseFilePath, releaseDownloadBytes);

                    var taskDebugDownload = githubClient.Connection.Get<byte[]>(new Uri(taskInfo.Release.DebugZip_DownloadURL), new Dictionary<string, string>(), "application/octet-stream");
                    byte[] debugDownloadBytes = taskDebugDownload.Result.Body;
                    taskInfo.AUM_DebugFilePath = Main.MOD_RELATIVE_PATH + "Debug.zip";
                    File.WriteAllBytes(taskInfo.AUM_DebugFilePath, debugDownloadBytes);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.WriteError("Exception occurred in DownloadWorker\n" + ex.Message + "\n" + taskInfo.IsLauncherUpdate.ToString() + "\n\n" + ex.StackTrace, rtxtLog, true);
                taskInfo.Success = false;
                e.Result = taskInfo;
                return;
            }
            taskInfo.Success = true;
            e.Result = taskInfo;
        }

        private void DownloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DownloadWorker_TaskInfo taskInfo = e.Result as DownloadWorker_TaskInfo;

            if (taskInfo.Success)
            {
                if (taskInfo.IsLauncherUpdate)
                {
                    Logger.Log.Write("Downloaded new launcher to " + taskInfo.Launcher_FilePath, Logger.ELogType.Notification, rtxtLog, true);
                    FinishInstallingNewLauncherRelease(taskInfo);
                }
                else
                {
                    Logger.Log.Write("Downloaded new AUM version (" + taskInfo.Release.TagSemver.ToString() + ")\n" + taskInfo.AUM_ReleaseFilePath + "\n" + taskInfo.AUM_DebugFilePath, Logger.ELogType.Notification, rtxtLog);
                    FinishInstallingNewMenuRelease(taskInfo);
                }
            }
        }
    }
}
