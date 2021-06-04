using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Octokit;
using Semver;

namespace aum_launcher
{
    public partial class Main
    {
        class WorkerProgress_ReleaseInfo
        {
            public SemVersion TagSemver = null;
            public string ReleaseZip_DownloadURL = "";
            public string DebugZip_DownloadURL = "";
            public string LauncherExe_DownloadURL = "";
            public bool HasNewerLauncherVersion = false;
            public SemVersion LauncherSemver = null;

            public WorkerProgress_ReleaseInfo() { }

            public WorkerProgress_ReleaseInfo(SemVersion tagSemver, string releaseDownloadURL, string debugDownloadURL, string launcherDownloadURL, bool hasNewerLauncher, SemVersion launcherSemver)
            {
                TagSemver = tagSemver;
                ReleaseZip_DownloadURL = releaseDownloadURL;
                DebugZip_DownloadURL = debugDownloadURL;
                LauncherExe_DownloadURL = launcherDownloadURL;
                HasNewerLauncherVersion = hasNewerLauncher;
                LauncherSemver = launcherSemver;
            }

            // probably not necessary but i don't care enough to find out
            public WorkerProgress_ReleaseInfo(WorkerProgress_ReleaseInfo other)
            {
                TagSemver = other.TagSemver;
                ReleaseZip_DownloadURL = other.ReleaseZip_DownloadURL;
                DebugZip_DownloadURL = other.DebugZip_DownloadURL;
                LauncherExe_DownloadURL = other.LauncherExe_DownloadURL;
                HasNewerLauncherVersion = other.HasNewerLauncherVersion;
            }

            public override string ToString()
            {
                return TagSemver.ToString();
            }
        }

        public const string GITHUB_OWNER = "BitCrackers";
        public const string GITHUB_REPOSITORY = "AmongUsMenu";
        public const string GITHUB_PRODUCT_HEADER = "BitCrackers-AmongUsMenu-Launcher";

        public GitHubClient githubClient;
        delegate Task<IReadOnlyList<Release>> dGetAllReleases(string owner, string repo, ApiOptions options);

        List<WorkerProgress_ReleaseInfo> AllRepoReleases = new List<WorkerProgress_ReleaseInfo>();
        WorkerProgress_ReleaseInfo LatestLauncherRelease = null;
        WorkerProgress_ReleaseInfo LatestMenuRelease = null;
        bool AUMUpdateAvailable = false;

        private void InitializeGithubClient()
        {
            githubClient = new GitHubClient(new ProductHeaderValue(GITHUB_PRODUCT_HEADER, LAUNCHER_VERSION));

            Type releaseClientType = typeof(ReleasesClient);
            System.Reflection.MethodInfo getAllReleasesMethodInfo = releaseClientType.GetMethod("GetAll", new Type[] { typeof(string), typeof(string), typeof(ApiOptions) });
            Logger.Log.Write("Got getAllReleasesMethodInfo: " + (getAllReleasesMethodInfo != null).ToString() + " = " + (getAllReleasesMethodInfo == null ? "<null>" : getAllReleasesMethodInfo.ToString()), Logger.ELogType.Info, rtxtLog);

            Delegate getAllReleasesBoundDel = Delegate.CreateDelegate(typeof(dGetAllReleases), githubClient.Repository.Release, getAllReleasesMethodInfo);
            GithubWorker.RunWorkerAsync(getAllReleasesBoundDel);
        }


        private void GithubWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AUMUpdateAvailable = false;

            dGetAllReleases getAllReleases = e.Argument as dGetAllReleases;
            Task<IReadOnlyList<Release>> releasesTask = getAllReleases(GITHUB_OWNER, GITHUB_REPOSITORY, ApiOptions.None);
            IReadOnlyList<Release> releases = releasesTask.Result;
            int numReleasesProcessed = 0;

            WorkerProgress_ReleaseInfo curReleaseInfo = new WorkerProgress_ReleaseInfo();
            foreach (Release release in releases)
            {
                numReleasesProcessed++;
                Logger.Log.Write("Saw release: " + release.TagName + "\n" + release.HtmlUrl, Logger.ELogType.Info, rtxtLog);
                // clear old releaseInfo
                curReleaseInfo.HasNewerLauncherVersion = false;
                curReleaseInfo.LauncherSemver = null;
                curReleaseInfo.TagSemver = null;
                curReleaseInfo.ReleaseZip_DownloadURL = "";
                curReleaseInfo.DebugZip_DownloadURL = "";
                curReleaseInfo.LauncherExe_DownloadURL = "";

                // parse semver out of tag, skipping the 'v' at the start.
                SemVersion releaseSemver;
                bool hasGoodParse = false;
                if (release.TagName.StartsWith("v"))
                    hasGoodParse = SemVersion.TryParse(release.TagName.Substring(1), out releaseSemver);
                else
                    hasGoodParse = SemVersion.TryParse(release.TagName, out releaseSemver);
                if (!hasGoodParse)
                {
                    Logger.Log.WriteError("Could not parse semver out of release tag (" + release.TagName + ")[" + release.HtmlUrl + "]", rtxtLog);
                    continue;
                }
                curReleaseInfo.TagSemver = releaseSemver;

                // process assets: save asset URLs and preliminary check for launcher version being greater than our current version
                foreach (ReleaseAsset asset in release.Assets)
                {
                    Logger.Log.Write("--Saw asset: " + asset.Name + "\n" + asset.BrowserDownloadUrl, Logger.ELogType.Info, rtxtLog);
                    // save all download URLs
                    if (asset.Name.StartsWith(LAUNCHER_NAME))
                    {
                        // TO-DO:
                        // change this to asset.Url once auto-updating is implemented
                        curReleaseInfo.LauncherExe_DownloadURL = asset.BrowserDownloadUrl;
                        // parse launcher semver, preliminary check if it's greater than current
                        string assetSemverStr = asset.Name.Remove(0, 12);
                        assetSemverStr = assetSemverStr.Substring(0, assetSemverStr.LastIndexOf('.'));
                        SemVersion assetSemver = null;
                        if (!SemVersion.TryParse(assetSemverStr, out assetSemver))
                        {
                            Logger.Log.WriteError("Could not parse semver from launcher asset (" + asset.Name + ")", rtxtLog);
                            continue;
                        }
                        if (assetSemver.CompareByPrecedence(LauncherCurrentSemver) > 0)
                        {
                            curReleaseInfo.HasNewerLauncherVersion = true;
                            curReleaseInfo.LauncherSemver = assetSemver;
                        }
                    }
                    else if (asset.Name.StartsWith("Release"))
                        curReleaseInfo.ReleaseZip_DownloadURL = asset.Url;
                    else if (asset.Name.StartsWith("Debug"))
                        curReleaseInfo.DebugZip_DownloadURL = asset.Url;
                }
                GithubWorker.ReportProgress((int)(((float)numReleasesProcessed / (float)releases.Count) * 100f), new WorkerProgress_ReleaseInfo(curReleaseInfo));
            }
            e.Result = releases;
        }

        private void GithubWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WorkerProgress_ReleaseInfo releaseInfo = e.UserState as WorkerProgress_ReleaseInfo;

            Logger.Log.Write("Adding release '" + releaseInfo.TagSemver.ToString() + "' to combobox and AllReleases list", Logger.ELogType.Info, rtxtLog);

            cboxTaggedVersion.Items.Add(releaseInfo);
            AllRepoReleases.Add(releaseInfo);

            // check against currently selected tag
            if (releaseInfo.TagSemver.CompareByPrecedence(ActiveProfile.LocalTaggedSemVer) > 0)
            {
                AUMUpdateAvailable = true;
            }

            // keep track of latest AUM version
            if (LatestMenuRelease == null)
            {
                LatestMenuRelease = releaseInfo;
            }
            else if (releaseInfo.TagSemver.CompareByPrecedence(LatestMenuRelease.TagSemver) > 0)
            {
                LatestMenuRelease = releaseInfo;
            }

            // check against latest launcher
            if (releaseInfo.HasNewerLauncherVersion)
            {
                if (LatestLauncherRelease == null)
                {
                    LatestLauncherRelease = releaseInfo;
                }
                else if (releaseInfo.LauncherSemver.CompareByPrecedence(LatestLauncherRelease.LauncherSemver) > 0)
                {
                    LatestLauncherRelease = releaseInfo;
                    Logger.Log.Write("Saw newer launcher version (" + releaseInfo.LauncherSemver.ToString() + " > " + LatestLauncherRelease.LauncherSemver + ")", Logger.ELogType.Info, rtxtLog);
                }
            }
        }

        private void GithubWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IReadOnlyList<Release> releases = e.Result as IReadOnlyList<Release>;
            Logger.Log.Write("GithubWorker finished fetching " + releases.Count.ToString() + " releases", Logger.ELogType.Info, rtxtLog);
            if (LatestLauncherRelease != null)
            {
                Logger.Log.Write("New Launcher update found! Please download the latest version at " + LatestLauncherRelease.LauncherExe_DownloadURL, Logger.ELogType.Notification, rtxtLog);
                lblLauncherVersion.ForeColor = System.Drawing.Color.Red;
                lblLauncherVersion.Text = "Launcher Version: " + LAUNCHER_VERSION + " (outdated)";
                StatusLbl_Launcher.ForeColor = System.Drawing.Color.Red;
                StatusLbl_Launcher.Text = "Launcher: Update!";
                btnUpdateLauncher.Enabled = true;
            }
            else
            {
                Logger.Log.Write("Launcher is up to date", Logger.ELogType.Info, rtxtLog);
                lblLauncherVersion.ForeColor = System.Drawing.Color.LimeGreen;
                lblLauncherVersion.Text = "Launcher Version: " + LAUNCHER_VERSION;
                StatusLbl_Launcher.ForeColor = System.Drawing.Color.LimeGreen;
                StatusLbl_Launcher.Text = "Launcher: OK";
                btnUpdateLauncher.Enabled = true;
            }

            if (AUMUpdateAvailable)
            {
                // notify user that they can select a newer build of AUM, but don't force it (they may be using an older build or a pre-release build on purpose)
                Logger.Log.Write("A newer version of AUM is available. Select it in the combobox to auto-download.", Logger.ELogType.Notification, rtxtLog);
                StatusLbl_AUM.ForeColor = System.Drawing.Color.DarkOrange;
                StatusLbl_AUM.Text = "AUM: Update!";
            }
            else
            {
                StatusLbl_AUM.ForeColor = System.Drawing.Color.LimeGreen;
                StatusLbl_AUM.Text = "AUM: OK";
            }
        }
    }
}
