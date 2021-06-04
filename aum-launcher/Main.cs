using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using System.Text.RegularExpressions;

namespace aum_launcher
{
    public partial class Main : Form
    {
        public const string LAUNCHER_VERSION = "1.0.0";
        public const string LAUNCHER_NAME = "AUMLauncher";
        public const string MESSAGEBOX_CAPTION = "BitCrackers - AUM Launcher";
        public const string MOD_FOLDER_NAME = "AmongUsMenu";
        public const string MOD_RELATIVE_PATH = MOD_FOLDER_NAME + "\\";
        public const string MOD_RELEASE_FOLDER_NAME = MOD_RELATIVE_PATH + "Release";
        public const string MOD_RELEASE_FOLDER_PATH = MOD_RELEASE_FOLDER_NAME + "\\";
        public const string MOD_DEBUG_FOLDER_NAME = MOD_RELATIVE_PATH + "Debug";
        public const string MOD_DEBUG_FOLDER_PATH = MOD_DEBUG_FOLDER_NAME + "\\";
        public const string MOD_VERSION_FILENAME = "version.dll";
        public const string MOD_INJECTABLE_FILENAME = "AmongUsMenu.dll";
        public const string PROFILE_FILE_EXT = ".aumprof";
        public const string GAME_EXE_NAME = "Among Us.exe";
        public const string GAMEASSEMBLY_DLL_NAME = "GameAssembly.dll";
        public const string GAME_STEAM_DEFAULT_PATH = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Among Us\\";
        public const string GAME_STEAM_RELATIVE_PATH = "Steam\\steamapps\\common\\Among Us\\";
        public const string GAME_STEAMLIBRARY_RELATIVE_PATH = "SteamLibrary\\steamapps\\common\\Among Us\\";

        public const int FORM_MAIN_SHRUNK_HEIGHT = 260;
        public const int FORM_MAIN_EXPANDED_HEIGHT = 472;


        private Profile m_ActiveProfile = new Profile();
        private Profile ActiveProfile
        {
            get { return m_ActiveProfile; }
            set { m_ActiveProfile = value; RefreshControlsState(); }
        }

        private readonly Semver.SemVersion LauncherCurrentSemver;

        private bool IsShrunk = false;

        public Main()
        {
            InitializeComponent();

            if (!Semver.SemVersion.TryParse(LAUNCHER_VERSION, out LauncherCurrentSemver))
            {
                Logger.Log.WriteError("Could not parse launcher's current semver string '" + LAUNCHER_VERSION + "'", rtxtLog, true);
                Application.Exit();
                return;
            }
        }

        void CheckForOldLaunchers()
        {
            foreach (string filename in Directory.EnumerateFiles(Directory.GetCurrentDirectory()))
            {
                if ((filename.Contains("AUMLauncher")) && (filename.EndsWith("_old")))
                {
                    // delete the old one
                    File.Delete(filename);
                    Logger.Log.Write("Finished updating launcher", Logger.ELogType.Notification, rtxtLog);
                    return;
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Logger.Log.Write("Launcher loading...", Logger.ELogType.Info, rtxtLog);
            lblLauncherVersion.ForeColor = System.Drawing.Color.Black;
            lblLauncherVersion.Text = "Launcher Version: " + LAUNCHER_VERSION;

            CheckForOldLaunchers();
            InitializeProfileSystem();
            GameDirWorker.RunWorkerAsync(ActiveProfile);

            // start github worker (first task is to fetch all releases and parse version tags)
            //InitializeGithubClient();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.Log.Write("Launcher closing...", Logger.ELogType.Info, rtxtLog);
            SaveProfileSystem();
        }

        private void RefreshControlsState()
        {
            cboxTaggedVersion.Text = ActiveProfile.LocalTaggedSemVer.ToString();
            lblCurrentBuild.Text = "Currently using: (" + (ActiveProfile.UseDebugBuild ? "Debug " : "Release ") + (ActiveProfile.UseProxyVersion ? "Proxy" : "Injectable") + ")";
            btnSwitchToProxy.Enabled = !ActiveProfile.UseProxyVersion;
            btnSwitchToInjectable.Enabled = ActiveProfile.UseProxyVersion;
        }

        private void MenuAbout_MenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("=== AUM Credits ===\n" +
                "void* (Maintainer &Creator)\n" +
                "std - nullptr(Maintainer & IL2CPP Madman)\n" +
                "OsOmE1(Maintainer)\n" +
                "RealMVC(Contributor)\n" +
                "Kyreus(Contributor)\n" +
                "manianac(Contributor)\n" +
                "=== Launcher Credits ===\n" +
                "kotae4\n");
        }

        private void HashWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string gameModuleFilePath = e.Argument as string;
            Logger.Log.Write("HashWorker beginning to hash '" + gameModuleFilePath + "'", Logger.ELogType.Info, rtxtLog);
            CRC32 crc = new CRC32();
            using (FileStream fstream = File.Open(gameModuleFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buf = new byte[fstream.Length];
                int numBytesRead = 0;
                long numBytesToRead = fstream.Length;
                while (numBytesToRead > 0)
                {
                    int read = fstream.Read(buf, numBytesRead, (numBytesToRead < 4096 ? (int)numBytesToRead : 4096));
                    crc.add(buf, numBytesRead, read);
                    numBytesRead += read;
                    numBytesToRead -= read;
                }
            }
            string crcHash = crc.getHash();
            e.Result = crcHash;
        }

        private void HashWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Error != null) || (e.Cancelled))
            {
                // TO-DO:
                // handle this
                Logger.Log.Write("HashWorker failed or was cancelled", Logger.ELogType.Notification, rtxtLog);
                return;
            }
            if (e.Result != null)
            {
                lblGameDetails.Text = "Game Checksum: " + (e.Result as string);
                Logger.Log.Write("HashWorker finished hashing: " + (e.Result as string), Logger.ELogType.Info, rtxtLog);
            }
        }

        private void GameDirWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Profile activeProf = e.Argument as Profile;
            if ((activeProf != null) && (!string.IsNullOrEmpty(activeProf.GameDirPath)))
            {
                Logger.Log.Write("Profile's GameDirPath: " + activeProf.GameDirPath, Logger.ELogType.Info, rtxtLog);
                if (Directory.Exists(activeProf.GameDirPath))
                {
                    e.Result = activeProf.GameDirPath;
                    Logger.Log.Write("GameDirWorker returning path from ActiveProfile", Logger.ELogType.Info, rtxtLog);
                    return;
                }
            }
            try
            {
                // 1. check if game exe is in current directory
                if (File.Exists(GAME_EXE_NAME))
                {
                    e.Result = Path.GetDirectoryName(Path.GetFullPath(GAME_EXE_NAME)) + "\\";
                    Logger.Log.Write("GameDirWorker returning path from local directory", Logger.ELogType.Info, rtxtLog);
                    return;
                }
                // 2. if not, check if C:\\Program Files (x86)\\Steam\\SteamApps\common\\Among Us\\Among Us.exe exists
                if (File.Exists(GAME_STEAM_DEFAULT_PATH + GAME_EXE_NAME))
                {
                    e.Result = GAME_STEAM_DEFAULT_PATH;
                    Logger.Log.Write("GameDirWorker returning path from steam default directory", Logger.ELogType.Info, rtxtLog);
                    return;
                }
                // 3. if not, enumerate disk drives and append Steam\\SteamApps\\common\\Among Us\\Among Us.exe and check if it exists (also check SteamLibrary\\ instead of Steam\\)
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady == false) continue;
                    if (File.Exists(drive.Name + GAME_STEAM_RELATIVE_PATH + GAME_EXE_NAME))
                    {
                        e.Result = drive.Name + GAME_STEAM_RELATIVE_PATH;
                        Logger.Log.Write("GameDirWorker returning path from drive " + drive.Name, Logger.ELogType.Info, rtxtLog);
                        return;
                    }
                    if (File.Exists(drive.Name + GAME_STEAMLIBRARY_RELATIVE_PATH + GAME_EXE_NAME))
                    {
                        e.Result = drive.Name + GAME_STEAMLIBRARY_RELATIVE_PATH;
                        Logger.Log.Write("GameDirWorker returning path from library on drive " + drive.Name, Logger.ELogType.Info, rtxtLog);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Alert("Exception occurred locating game install directory:\n" + ex.Message + "\n\n" + ex.StackTrace, MESSAGEBOX_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error, Logger.ELogType.Exception, rtxtLog);
                e.Result = string.Empty;
                return;
            }
        }

        private void GameDirWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Result as string))
            {
                // 4. if not, prompt the user to locate their game install directory
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(folderBrowserDialog1.SelectedPath + "\\" + GAME_EXE_NAME))
                    {
                        ActiveProfile.GameDirPath = folderBrowserDialog1.SelectedPath + "\\";
                        Logger.Log.Write("Got GameDir from user: " + ActiveProfile.GameDirPath, Logger.ELogType.Notification, rtxtLog);
                        // start background worker to hash game exe
                        HashWorker.RunWorkerAsync(ActiveProfile.GameDirPath + GAMEASSEMBLY_DLL_NAME);
                        // start up the ProcessWatcher too
                        SetWatchedProcess(ActiveProfile.GameDirPath + GAME_EXE_NAME);
                        return;
                    }
                    Logger.Log.Write("User selected wrong game directory when prompted (" + folderBrowserDialog1.SelectedPath + ")", Logger.ELogType.Info, rtxtLog);
                }
                Logger.Log.WriteError("Could not locate Among Us directory", rtxtLog, true);
                Application.Exit();
            }
            else
            {
                ActiveProfile.GameDirPath = e.Result as string;
                Logger.Log.Write("Got GameDir automatically: " + ActiveProfile.GameDirPath, Logger.ELogType.Notification, rtxtLog);
                // start background worker to hash game exe
                HashWorker.RunWorkerAsync(ActiveProfile.GameDirPath + GAMEASSEMBLY_DLL_NAME);
                // start up the ProcessWatcher too
                SetWatchedProcess(ActiveProfile.GameDirPath + GAME_EXE_NAME);
            }
        }

        private void cboxTaggedVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            // THIS ACTUALLY WORKS LOL
            // THANK YOU WINFORMS DEVS
            WorkerProgress_ReleaseInfo selectedRelease = (WorkerProgress_ReleaseInfo)cboxTaggedVersion.SelectedItem;

            Logger.Log.Write("User selected " + selectedRelease.TagSemver.ToString() + " from combobox\nReleaseURL: " + selectedRelease.ReleaseZip_DownloadURL + "\nDebugURL: " + selectedRelease.DebugZip_DownloadURL, Logger.ELogType.Info, rtxtLog);
            DownloadAndInstallMenuFromRelease(selectedRelease);
        }

        private void btnUpdateLauncher_Click(object sender, EventArgs e)
        {
            if (LatestLauncherRelease == null)
                return;
            btnUpdateLauncher.Enabled = false;
            Logger.Log.Write("Beginning launcher update (the launcher will auto-restart)...", Logger.ELogType.Notification, rtxtLog);
            DownloadAndInstallLauncherFromRelease(LatestLauncherRelease);
        }

        private void btnSwitchToProxy_Click(object sender, EventArgs e)
        {
            ActiveProfile.UseProxyVersion = true;
            btnInject.Enabled = false;
            btnSwitchToProxy.Enabled = false;
            btnSwitchToInjectable.Enabled = true;
            if (File.Exists(ActiveProfile.GameDirPath + MOD_VERSION_FILENAME))
            {
                Logger.Log.Write("version.dll already exists in game directory, deleting it and copying fresh", Logger.ELogType.Info, rtxtLog);
                File.Delete(ActiveProfile.GameDirPath + MOD_VERSION_FILENAME);
            }
            File.Copy((ActiveProfile.UseDebugBuild ? MOD_DEBUG_FOLDER_PATH : MOD_RELEASE_FOLDER_PATH) + MOD_VERSION_FILENAME, ActiveProfile.GameDirPath + MOD_VERSION_FILENAME);
            Logger.Log.Write("Installed proxy version.dll from '" + (ActiveProfile.UseDebugBuild ? MOD_DEBUG_FOLDER_PATH : MOD_RELEASE_FOLDER_PATH) + MOD_VERSION_FILENAME + "' to '" + ActiveProfile.GameDirPath + MOD_VERSION_FILENAME + "'", Logger.ELogType.Info, rtxtLog);
            lblCurrentBuild.Text = "Currently using: (" + (ActiveProfile.UseDebugBuild ? "Debug " : "Release ") + (ActiveProfile.UseProxyVersion ? "Proxy" : "Injectable") + ")";
        }

        private void btnSwitchToInjectable_Click(object sender, EventArgs e)
        {
            if ((ActiveProfile.UseProxyVersion == true) && ((SelectedProcess.ActiveProcess != null) && (SelectedProcess.ActiveProcess.HasExited == false)))
            {
                Logger.Log.WriteError("User tried to switch to injectable version while game is running. Please close the game first!", rtxtLog);
                return;
            }
            ActiveProfile.UseProxyVersion = false;
            btnSwitchToInjectable.Enabled = false;
            btnSwitchToProxy.Enabled = true;
            if (File.Exists(ActiveProfile.GameDirPath + MOD_VERSION_FILENAME))
            {
                Logger.Log.Write("Deleting version.dll from game directory '" + ActiveProfile.GameDirPath + MOD_VERSION_FILENAME + "'", Logger.ELogType.Info, rtxtLog);
                File.Delete(ActiveProfile.GameDirPath + MOD_VERSION_FILENAME);
            }
            else
            {
                Logger.Log.Write("Could not find version.dll in game directory '" + ActiveProfile.GameDirPath + MOD_VERSION_FILENAME + "'", Logger.ELogType.Info, rtxtLog);
            }
            btnInject.Enabled = true;
            lblCurrentBuild.Text = "Currently using: (" + (ActiveProfile.UseDebugBuild ? "Debug " : "Release ") + (ActiveProfile.UseProxyVersion ? "Proxy" : "Injectable") + ")";
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            if ((SelectedProcess == null) || (SelectedProcess.ActiveProcess == null) || (SelectedProcess.ActiveProcess.HasExited) || (ActiveProfile.UseProxyVersion))
            {
                Logger.Log.Write("Could not inject because the game is not running", Logger.ELogType.Notification, rtxtLog);
                return;
            }

            string pathToDll = (ActiveProfile.UseDebugBuild ? MOD_DEBUG_FOLDER_PATH : MOD_RELEASE_FOLDER_PATH) + MOD_INJECTABLE_FILENAME;
            if ((Path.IsPathRooted(pathToDll) == false) || (pathToDll.StartsWith(":\\") == false))
                pathToDll = Path.GetFullPath(pathToDll);

            if (File.Exists(pathToDll) == false)
            {
                Logger.Log.Write("Could not inject because the DLL is missing (antivirus deleted it?)", Logger.ELogType.Notification, rtxtLog);
                return;
            }

            Logger.Log.Write("Injecting...", Logger.ELogType.Notification, rtxtLog);
            Injector.Inject(SelectedProcess.ActiveProcess, pathToDll, rtxtLog);
            Logger.Log.Write("Successfully injected!", Logger.ELogType.Notification, rtxtLog, true);
            StatusLbl_Injection.ForeColor = System.Drawing.Color.LimeGreen;
            StatusLbl_Injection.Text = "Success!";
        }

        private void chboxUseDebugBuild_CheckedChanged(object sender, EventArgs e)
        {
            ActiveProfile.UseDebugBuild = chboxUseDebugBuild.Checked;
            lblCurrentBuild.Text = "Currently using: (" + (ActiveProfile.UseDebugBuild ? "Debug " : "Release ") + (ActiveProfile.UseProxyVersion ? "Proxy" : "Injectable") + ")";
        }

        private void btnToggleLogDisplay_Click(object sender, EventArgs e)
        {
            Size = new Size(Size.Width, (IsShrunk ? FORM_MAIN_EXPANDED_HEIGHT : FORM_MAIN_SHRUNK_HEIGHT));
            IsShrunk = !IsShrunk;
        }
    }
}
