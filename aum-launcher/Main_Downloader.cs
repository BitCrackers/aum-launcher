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
            public GithubWorker_TaskProgress_ReleaseInfo Release;
            public bool IsLauncherUpdate;

            public string AUM_ReleaseFilePath = "";
            public string AUM_DebugFilePath = "";
            public string Launcher_FilePath = "";

            public bool Success = false;

            public DownloadWorker_TaskInfo(GithubWorker_TaskProgress_ReleaseInfo release, bool isLauncherUpdate)
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

        void DownloadAndInstallMenuFromRelease(GithubWorker_TaskProgress_ReleaseInfo release)
        {
            IsDownloadingMenuUpdate = true;
            while (DownloadWorker.IsBusy)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            Logger.Log.Write("Beginning menu download", Logger.ELogType.Info, rtxtLog);
            DownloadWorker.RunWorkerAsync(new DownloadWorker_TaskInfo(release, false));
        }

        void DownloadAndInstallLauncherFromRelease(GithubWorker_TaskProgress_ReleaseInfo release)
        {
            IsDownloadingLauncherUpdate = true;
            if (File.Exists(Process.GetCurrentProcess().MainModule.FileName))
            {
                // move the current launcher files to its own 'old' directory, then pass some arguments to the new launcher exe to progress the update step.
                // WARNING:
                // begins the assumption that the launcher makes on startup to search for old installations
                Directory.CreateDirectory(LAUNCHER_DELETION_DIR);
                foreach (string oldLauncherFile in LAUNCHER_FILES)
                {
                    File.Move(oldLauncherFile, LAUNCHER_DELETION_DIR + "\\" + oldLauncherFile);
                    Logger.Log.Write("Moved '" + oldLauncherFile + "' to deletion directory in preparation for update", Logger.ELogType.Info, rtxtLog);
                }
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
            Logger.Log.Write("Beginning launcher download", Logger.ELogType.Info, rtxtLog);
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
            // WARNING:
            // assumes the release asset is a zip file
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
            // WARNING:
            // assumes the release asset is a zip file
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
            // TO-DO:
            // if currently using proxy version, we should delete version.dll from gamedir and copy this new version over
        }

        void FinishInstallingNewLauncherRelease(DownloadWorker_TaskInfo taskInfo)
        {
            Logger.Log.Write("Finished downloading launcher update", Logger.ELogType.Notification, rtxtLog, true);
            // unzip to LAUNCHER_UPDATE_DIR
            if (Directory.Exists(LAUNCHER_UPDATE_DIR))
            {
                Logger.Log.Write("Clearing " + LAUNCHER_UPDATE_DIR + " of existing files", Logger.ELogType.Info, rtxtLog);
                foreach (string filename in Directory.EnumerateFiles(LAUNCHER_UPDATE_DIR))
                {
                    File.Delete(filename);
                }
            }
            else
            {
                Logger.Log.Write("Creating new " + LAUNCHER_UPDATE_DIR + " directory", Logger.ELogType.Info, rtxtLog);
                Directory.CreateDirectory(LAUNCHER_UPDATE_DIR);
            }
            // WARNING:
            // assumes the launcher asset is a zip file
            ZipFile.ExtractToDirectory(taskInfo.Launcher_FilePath, LAUNCHER_UPDATE_DIR);
            File.Delete(taskInfo.Launcher_FilePath);
            Logger.Log.Write("Extracted release assets to " + LAUNCHER_UPDATE_DIR, Logger.ELogType.Info, rtxtLog);
            // start new launcher process... this starts a chain reaction where this launcher exits, the new launcher creates *another* new instance and also exits
            // it's all caused by not being able to delete currently running files
            
            string args = "-update ";
            // we pass our current pid so the new launcher can wait on us to exit
            int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            args += pid.ToString() + " ";
            // WARNING: 
            // this may be bad. might be a better idea to cache WorkingDirectory on startup.
            string currentDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            // pass the current directory so the new launcher process knows where to install itself to
            args += currentDirectory + " ";
            // pass the deletion dir (where the current launcher files should be) so they can be deleted
            args += LAUNCHER_DELETION_DIR;
            // we have to find the exe too... 
            // WARNING:
            // this assumes there's only ever 1 file with the exe extension as part of each launcher release
            // (the most limiting assumption)
            string launcherFilename = null;
            foreach (string filename in Directory.EnumerateFiles(LAUNCHER_UPDATE_DIR))
            {
                if (filename.EndsWith(".exe"))
                {
                    launcherFilename = filename;
                    break;
                }
            }
            if (string.IsNullOrEmpty(launcherFilename))
            {
                Logger.Log.WriteError("Could not find updated launcher exe, cannot continue update. Files will be left behind! Feel free to delete them.", rtxtLog, true);
                // TO-DO:
                // move current launcher files from LAUNCHER_DELETION_DIR back to currentDirectory
                return;
            }
            Process newLauncherProc = Process.Start(launcherFilename, args);
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
                    Logger.Log.Write("Downloading '" + taskInfo.Release.LauncherExe_DownloadURL + "'...", Logger.ELogType.Info, rtxtLog);
                    var taskDownload = githubClient.Connection.Get<byte[]>(new Uri(taskInfo.Release.LauncherExe_DownloadURL), new Dictionary<string, string>(), "application/octet-stream");
                    byte[] downloadBytes = taskDownload.Result.Body;
                    // NOTE:
                    // SemVer.ToString() will output dots. we could change these to '_' for more clarity.
                    taskInfo.Launcher_FilePath = Main.LAUNCHER_NAME + "-" + taskInfo.Release.TagSemver.ToString() + ".zip";
                    File.WriteAllBytes(taskInfo.Launcher_FilePath, downloadBytes);
                    Logger.Log.Write("Saved " + downloadBytes.Length.ToString() + " bytes to file '" + taskInfo.Launcher_FilePath + "'", Logger.ELogType.Info, rtxtLog);
                }
                else
                {
                    Logger.Log.Write("Downloading '" + taskInfo.Release.ReleaseZip_DownloadURL + "'...", Logger.ELogType.Info, rtxtLog);
                    var taskReleaseDownload = githubClient.Connection.Get<byte[]>(new Uri(taskInfo.Release.ReleaseZip_DownloadURL), new Dictionary<string, string>(), "application/octet-stream");
                    byte[] releaseDownloadBytes = taskReleaseDownload.Result.Body;
                    taskInfo.AUM_ReleaseFilePath = Main.MOD_RELATIVE_PATH + "Release.zip";
                    File.WriteAllBytes(taskInfo.AUM_ReleaseFilePath, releaseDownloadBytes);
                    Logger.Log.Write("Saved " + releaseDownloadBytes.Length.ToString() + " bytes to file '" + taskInfo.AUM_ReleaseFilePath + "'", Logger.ELogType.Info, rtxtLog);

                    Logger.Log.Write("Downloading '" + taskInfo.Release.DebugZip_DownloadURL + "'...", Logger.ELogType.Info, rtxtLog);
                    var taskDebugDownload = githubClient.Connection.Get<byte[]>(new Uri(taskInfo.Release.DebugZip_DownloadURL), new Dictionary<string, string>(), "application/octet-stream");
                    byte[] debugDownloadBytes = taskDebugDownload.Result.Body;
                    taskInfo.AUM_DebugFilePath = Main.MOD_RELATIVE_PATH + "Debug.zip";
                    File.WriteAllBytes(taskInfo.AUM_DebugFilePath, debugDownloadBytes);
                    Logger.Log.Write("Saved " + debugDownloadBytes.Length.ToString() + " bytes to file '" + taskInfo.AUM_DebugFilePath + "'", Logger.ELogType.Info, rtxtLog);
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
