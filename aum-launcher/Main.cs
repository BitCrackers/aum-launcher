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

namespace aum_launcher
{
    // TO-DO:
    // [x] fetch all releases from github
    // [x] determine which releases are new
    // [x] hash game in the same way as AUM for helpful diagnostic info
    // [x] allow auto-downloading and updating of AUM
    // [x] allow auto-downloading and updating of the launcher itself
    // [x] switch launcher to its own repo (so we have to support interacting with two repos instead of just the one)
    // [x] auto-detect when the game process is running
    // [x] inject into game process regardless of launcher bitness and game bitness
    //     * [x] externally parse EAT for loadlibrary address
    // [x] log all the things! both to a log file and to a textbox on the form
    // [ ] expand logger to handle locked files better, maybe cache all log messages until a handle to the log file can be acquired
    // [ ] make state machine for all state transitions, this should help make the logic clearer
    //     * initial design ideas: State.GetNextTransitionState for automatic transitions, State.CanTransitionTo for user-initiated transitions
    //     * not sure how to translate this into cleaner code, though..
    //     * Main's controls are private so it'd just add a lot of boring code to be able to split logic into separate classes
    // [ ] add option to launch game from within the... launcher
    //     * this was originally meant to be an auto-updater and injector, 'launcher' is kind of a misnomer, but i guess we could add this functionality
    // [ ] add option to auto-close the launcher when it detects the game has started
    // [ ] make Profile fields into properties and have the setters call RefreshControlsState() for easy GUI updating

    public partial class Main : Form
    {
        public const string LAUNCHER_VERSION = "1.0.4";
        public const string LAUNCHER_NAME = "aum-launcher";
        // begin update info
        public const string LAUNCHER_DELETION_DIR = "AUMLAUNCHER_DELETE";
        public const string LAUNCHER_UPDATE_DIR = "AUMLAUNCHER_UPDATE";
        // WARNING:
        // !!!!! make sure this is kept up to date !!!!!
        public static readonly string[] LAUNCHER_FILES = new string[6] { "aum-launcher.exe", "aum-launcher.exe.config", "Octokit.dll", "Octokit.xml", "Semver.dll", "Semver.xml" };
        // end update info
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

        // i hate this hahahahahahaa
        // kill me
        // TO-DO:
        // clean this. bleach. ammonia. breathe heavily.
        bool CheckForOldLaunchers()
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args) { Logger.Log.Write("Saw commandline arg: " + arg, Logger.ELogType.Info, rtxtLog); }
            // WARNING:
            // assumes:
            // args[0] = the path of this exe (default arg, not sent by us)
            // args[1] = "-update"
            // args[2] = "<pid of parent process>"
            // args[3] = "path\\to\\install\\dir"
            // args[4] = "path\\to\\deletion\\dir"
            if (args.Length >= 5)
            {
                if (args[1] == "-update")
                {
                    Logger.Log.Write("Beginning second step of launcher update process", Logger.ELogType.Info, rtxtLog, true);
                    int parentPID = -1;
                    if (!int.TryParse(args[2], out parentPID))
                    {
                        Logger.Log.WriteError("Could not parse parent PID during launcher auto-update process, cannot continue update. Files will be left over! Feel free to delete them.", rtxtLog, true);
                        return true;
                    }
                    System.Diagnostics.Process parentProcess = System.Diagnostics.Process.GetProcessById(parentPID);
                    if (parentProcess == null)
                        Logger.Log.Write("Parent process not found, assuming it already exited", Logger.ELogType.Info, rtxtLog, true);
                    else
                    {
                        Logger.Log.Write("Waiting for parent process to close...", Logger.ELogType.Info, rtxtLog, true);
                        parentProcess.WaitForExit();
                    }
                    parentProcess.Close();
                    Logger.Log.Write("Parent process closed", Logger.ELogType.Info, rtxtLog, true);
                    string currentDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    if (Directory.Exists(args[3]))
                    {
                        // the old launcher needs to extract the new update to its own directory
                        // so, we have to move the newly updated install (the currently executing one) to the proper install dir
                        Logger.Log.Write("Beginning the process of moving ourselves to proper install dir. CurrentDirectory: " + currentDirectory, Logger.ELogType.Info, rtxtLog, true);
                        foreach (string launcherFile in LAUNCHER_FILES)
                        {
                            Logger.Log.Write("Moving " + currentDirectory + "\\" + launcherFile + " to " + args[3] + "\\" + launcherFile, Logger.ELogType.Info, rtxtLog, true);
                            File.Move(currentDirectory + "\\" + launcherFile, args[3] + "\\" + launcherFile);
                        }
                        Logger.Log.Write("Done moving launcher update to proper install dir", Logger.ELogType.Info, rtxtLog, true);
                        // moving ourselves can create some issues, so let's re-launch (a third time?) from the proper install dir and then clean up the old folders
                        int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                        string newArgs = "-deleteOldInstall " + pid.ToString() + " " + currentDirectory + " " + args[4];
                        Logger.Log.Write("Starting newly installed launcher update in proper dir with args: " + newArgs, Logger.ELogType.Info, rtxtLog, true);
                        System.Diagnostics.Process proc = System.Diagnostics.Process.Start(args[3] + "\\" + LAUNCHER_FILES[0], newArgs);
                        Logger.Log.Write("Exiting updated launcher (intermediate)", Logger.ELogType.Info, rtxtLog, true);
                        System.Windows.Forms.Application.Exit();
                        return true;
                    }
                }
                // WARNING:
                // assumes:
                // args[0] = the path of this exe (default arg, not sent by us)
                // args[1] = "-deleteOldInstall"
                // args[2] = <parent PID>
                // args[3] = "path\\to\\update\\dir"
                // args[4] = "path\\to\\deletion\\dir"
                else if (args[1] == "-deleteOldInstall")
                {
                    Logger.Log.Write("Beginning final step of update process", Logger.ELogType.Info, rtxtLog, true);
                    int parentPID = -1;
                    if (!int.TryParse(args[2], out parentPID))
                    {
                        Logger.Log.WriteError("Could not parse parent PID during launcher auto-update process, cannot continue update. Files will be left over! Feel free to delete them.", rtxtLog, true);
                        return true;
                    }
                    System.Diagnostics.Process parentProcess = System.Diagnostics.Process.GetProcessById(parentPID);
                    if (parentProcess == null)
                        Logger.Log.Write("Parent process not found, assuming it already exited", Logger.ELogType.Info, rtxtLog, true);
                    else
                    {
                        Logger.Log.Write("Waiting for parent process to close...", Logger.ELogType.Info, rtxtLog, true);
                        parentProcess.WaitForExit();
                    }
                    parentProcess.Close();
                    Logger.Log.Write("Parent process closed", Logger.ELogType.Info, rtxtLog, true);
                    if (Directory.Exists(args[3]))
                    {
                        Directory.Delete(args[3], true);
                    }
                    if (Directory.Exists(args[4]))
                    {
                        Directory.Delete(args[4], true);
                    }
                    Logger.Log.Write("Done cleaning up", Logger.ELogType.Info, rtxtLog, true);
                }
            }
            Logger.Log.Write("CheckForOldLaunchers() is returning to normal execution", Logger.ELogType.Info, rtxtLog, true);
            return false;
        }

        /*
        void MockInstallLauncherUpdate()
        {
            string currentDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            string deleteDirectory = currentDirectory + "\\LAUNCHER_DELETE";
            string installDirectory = currentDirectory + "\\LAUNCHER_UPDATE";

            // install new update (we do this first in the mock test because we're just copying ourselves, not actually downloading a separate release)
            Logger.Log.Write("Copying self to '" + installDirectory + "' as part of MockInstall", Logger.ELogType.Info, rtxtLog, true);
            if (Directory.Exists(installDirectory))
                Directory.Delete(installDirectory, true);
            Directory.CreateDirectory(installDirectory);
            foreach (string launcherFile in LAUNCHER_FILES)
                File.Copy(launcherFile, installDirectory + "\\" + launcherFile);

            // move current install to deletion directory
            Logger.Log.Write("Moving self to '" + deleteDirectory + " as part of MockInstall", Logger.ELogType.Info, rtxtLog, true);
            if (Directory.Exists(deleteDirectory))
                Directory.Delete(deleteDirectory, true);
            Directory.CreateDirectory(deleteDirectory);
            foreach (string oldLauncherFile in LAUNCHER_FILES)
                File.Move(oldLauncherFile, deleteDirectory + "\\" + oldLauncherFile);

            // normally the download & extraction would occur here...

            // start new launcher process
            int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            string args = "-update ";
            args += pid.ToString() + " ";
            args += currentDirectory + " ";
            args += deleteDirectory;
            Logger.Log.Write("Starting newly updated launcher (" + installDirectory + "\\" + LAUNCHER_FILES[0] + ") with args: " + args, Logger.ELogType.Info, rtxtLog, true);
            System.Diagnostics.Process.Start(installDirectory + "\\" + LAUNCHER_FILES[0], args);
            Logger.Log.Write("Exiting old launcher", Logger.ELogType.Info, rtxtLog, true);
            System.Windows.Forms.Application.Exit();
        }
        */

        private void Main_Load(object sender, EventArgs e)
        {
            Logger.Log.Write("Launcher loading...", Logger.ELogType.Info, rtxtLog);
            lblLauncherVersion.ForeColor = System.Drawing.Color.Black;
            lblLauncherVersion.Text = "Launcher Version: " + LAUNCHER_VERSION;

            Logger.Log.Write("Current process directory: " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, Logger.ELogType.Info, rtxtLog, true);

            InitializeProfileSystem();
            if (CheckForOldLaunchers()) return;
            
            GameDirWorker.RunWorkerAsync(ActiveProfile);

            // start github worker (first task is to fetch all releases and parse version tags)
            InitializeGithubClient();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.Log.Write("Launcher closing...", Logger.ELogType.Info, rtxtLog);
            SaveProfileSystem();
            Logger.Log.Close();
            // NOTE:
            // should we delete version.dll from the game directory? i'm thinking not, in case the user wants to launch the game without loading this updater
        }

        private void RefreshControlsState()
        {
            // this is kind of a useless function, need to expand its idea more
            // TO-DO:
            // make every change to ActiveProfile call this function. easy GUI updating.
            cboxTaggedVersion.Text = ActiveProfile.LocalTaggedSemVer.ToString();
            lblCurrentBuild.Text = "Currently using: (" + (ActiveProfile.UseDebugBuild ? "Debug " : "Release ") + (ActiveProfile.UseProxyVersion ? "Proxy" : "Injectable") + ")";
            btnSwitchToProxy.Enabled = !ActiveProfile.UseProxyVersion;
            btnSwitchToInjectable.Enabled = ActiveProfile.UseProxyVersion;
            if (ActiveProfile.UseProxyVersion)
                btnInject.Enabled = false;
            else
                btnInject.Enabled = SelectedProcess != null && SelectedProcess.ActiveProcess != null;
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
            e.Result = crc.getHash();
        }

        private void HashWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Error != null) || (e.Cancelled))
            {
                Logger.Log.Write("HashWorker failed or was cancelled", Logger.ELogType.Notification, rtxtLog);
                return;
            }
            if (e.Result != null)
            {
                lblGameDetails.Text = "Game Checksum: " + (e.Result as string);
                Logger.Log.Write("HashWorker finished hashing: " + (e.Result as string), Logger.ELogType.Info, rtxtLog);
            }
        }

        private void cboxTaggedVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            // THIS ACTUALLY WORKS LOL
            // THANK YOU WINFORMS DEVS
            GithubWorker_TaskProgress_ReleaseInfo selectedRelease = (GithubWorker_TaskProgress_ReleaseInfo)cboxTaggedVersion.SelectedItem;

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
            if ((string.IsNullOrEmpty(ActiveProfile.GameDirPath)) || (Directory.Exists(ActiveProfile.GameDirPath) == false))
                return;

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
                // we need to be able to delete version.dll, and if the game is running we can't do that, so this is an error.
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
            btnInject.Enabled = SelectedProcess != null && SelectedProcess.ActiveProcess != null;
            lblCurrentBuild.Text = "Currently using: (" + (ActiveProfile.UseDebugBuild ? "Debug " : "Release ") + (ActiveProfile.UseProxyVersion ? "Proxy" : "Injectable") + ")";
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            if ((SelectedProcess == null) || (SelectedProcess.ActiveProcess == null) || (SelectedProcess.ActiveProcess.HasExited) || (ActiveProfile.UseProxyVersion))
            {
                Logger.Log.Write("Could not inject because the game is not running or user has chosen to use the proxy version", Logger.ELogType.Notification, rtxtLog);
                return;
            }

            string pathToDll = (ActiveProfile.UseDebugBuild ? MOD_DEBUG_FOLDER_PATH : MOD_RELEASE_FOLDER_PATH) + MOD_INJECTABLE_FILENAME;
            if ((Path.IsPathRooted(pathToDll) == false) || (pathToDll.IndexOf(":\\") != 1))
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
            StatusLbl_Injection.Text = "Injection: Success!";
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
