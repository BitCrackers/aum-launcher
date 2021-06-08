
namespace aum_launcher
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.MenuStrip_Main = new System.Windows.Forms.MenuStrip();
            this.Menu_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuAbout_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBoxProcessIcon = new System.Windows.Forms.PictureBox();
            this.lblProcessDetails = new System.Windows.Forms.Label();
            this.lblGameDetails = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnInject = new System.Windows.Forms.Button();
            this.cboxTaggedVersion = new System.Windows.Forms.ComboBox();
            this.btnToggleLogDisplay = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chboxUseDebugBuild = new System.Windows.Forms.CheckBox();
            this.btnSwitchToInjectable = new System.Windows.Forms.Button();
            this.btnSwitchToProxy = new System.Windows.Forms.Button();
            this.lblCurrentBuild = new System.Windows.Forms.Label();
            this.btnUpdateLauncher = new System.Windows.Forms.Button();
            this.lblLauncherVersion = new System.Windows.Forms.Label();
            this.StatusStrip_Main = new System.Windows.Forms.StatusStrip();
            this.StatusLbl_Launcher = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLbl_AUM = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLbl_Game = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLbl_Injection = new System.Windows.Forms.ToolStripStatusLabel();
            this.ToolTip_Main = new System.Windows.Forms.ToolTip(this.components);
            this.rtxtLog = new System.Windows.Forms.RichTextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.ProcessPollWorker = new System.ComponentModel.BackgroundWorker();
            this.HashWorker = new System.ComponentModel.BackgroundWorker();
            this.GithubWorker = new System.ComponentModel.BackgroundWorker();
            this.GameDirWorker = new System.ComponentModel.BackgroundWorker();
            this.DownloadWorker = new System.ComponentModel.BackgroundWorker();
            this.MenuStrip_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProcessIcon)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.StatusStrip_Main.SuspendLayout();
            this.SuspendLayout();
            // 
            // MenuStrip_Main
            // 
            this.MenuStrip_Main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_MenuItem});
            this.MenuStrip_Main.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip_Main.Name = "MenuStrip_Main";
            this.MenuStrip_Main.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.MenuStrip_Main.Size = new System.Drawing.Size(399, 24);
            this.MenuStrip_Main.TabIndex = 0;
            this.MenuStrip_Main.Text = "menuStrip1";
            // 
            // Menu_MenuItem
            // 
            this.Menu_MenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuAbout_MenuItem});
            this.Menu_MenuItem.Name = "Menu_MenuItem";
            this.Menu_MenuItem.Size = new System.Drawing.Size(50, 20);
            this.Menu_MenuItem.Text = "Menu";
            // 
            // MenuAbout_MenuItem
            // 
            this.MenuAbout_MenuItem.Name = "MenuAbout_MenuItem";
            this.MenuAbout_MenuItem.Size = new System.Drawing.Size(107, 22);
            this.MenuAbout_MenuItem.Text = "About";
            this.MenuAbout_MenuItem.Click += new System.EventHandler(this.MenuAbout_MenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(107, 22);
            this.toolStripMenuItem2.Text = "About";
            // 
            // pictureBoxProcessIcon
            // 
            this.pictureBoxProcessIcon.Location = new System.Drawing.Point(10, 23);
            this.pictureBoxProcessIcon.Name = "pictureBoxProcessIcon";
            this.pictureBoxProcessIcon.Size = new System.Drawing.Size(27, 28);
            this.pictureBoxProcessIcon.TabIndex = 1;
            this.pictureBoxProcessIcon.TabStop = false;
            // 
            // lblProcessDetails
            // 
            this.lblProcessDetails.AutoSize = true;
            this.lblProcessDetails.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblProcessDetails.Location = new System.Drawing.Point(43, 23);
            this.lblProcessDetails.Name = "lblProcessDetails";
            this.lblProcessDetails.Size = new System.Drawing.Size(96, 13);
            this.lblProcessDetails.TabIndex = 2;
            this.lblProcessDetails.Text = "Waiting for game...";
            // 
            // lblGameDetails
            // 
            this.lblGameDetails.AutoSize = true;
            this.lblGameDetails.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblGameDetails.Location = new System.Drawing.Point(43, 38);
            this.lblGameDetails.Name = "lblGameDetails";
            this.lblGameDetails.Size = new System.Drawing.Size(124, 13);
            this.lblGameDetails.TabIndex = 3;
            this.lblGameDetails.Text = "Game Checksum: (none)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnInject);
            this.groupBox1.Controls.Add(this.cboxTaggedVersion);
            this.groupBox1.Controls.Add(this.btnToggleLogDisplay);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.chboxUseDebugBuild);
            this.groupBox1.Controls.Add(this.btnSwitchToInjectable);
            this.groupBox1.Controls.Add(this.btnSwitchToProxy);
            this.groupBox1.Controls.Add(this.lblCurrentBuild);
            this.groupBox1.Controls.Add(this.btnUpdateLauncher);
            this.groupBox1.Controls.Add(this.lblLauncherVersion);
            this.groupBox1.Location = new System.Drawing.Point(10, 56);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(378, 142);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // btnInject
            // 
            this.btnInject.Location = new System.Drawing.Point(247, 68);
            this.btnInject.Name = "btnInject";
            this.btnInject.Size = new System.Drawing.Size(53, 69);
            this.btnInject.TabIndex = 11;
            this.btnInject.Text = "Inject";
            this.btnInject.UseVisualStyleBackColor = true;
            this.btnInject.Click += new System.EventHandler(this.btnInject_Click);
            // 
            // cboxTaggedVersion
            // 
            this.cboxTaggedVersion.FormattingEnabled = true;
            this.cboxTaggedVersion.Location = new System.Drawing.Point(77, 38);
            this.cboxTaggedVersion.Name = "cboxTaggedVersion";
            this.cboxTaggedVersion.Size = new System.Drawing.Size(104, 21);
            this.cboxTaggedVersion.TabIndex = 10;
            this.cboxTaggedVersion.SelectedIndexChanged += new System.EventHandler(this.cboxTaggedVersion_SelectedIndexChanged);
            // 
            // btnToggleLogDisplay
            // 
            this.btnToggleLogDisplay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnToggleLogDisplay.Location = new System.Drawing.Point(172, 117);
            this.btnToggleLogDisplay.Name = "btnToggleLogDisplay";
            this.btnToggleLogDisplay.Size = new System.Drawing.Size(46, 20);
            this.btnToggleLogDisplay.TabIndex = 6;
            this.btnToggleLogDisplay.Text = "↕ ↕ ↕";
            this.btnToggleLogDisplay.UseVisualStyleBackColor = true;
            this.btnToggleLogDisplay.Click += new System.EventHandler(this.btnToggleLogDisplay_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "AUM Version:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::aum_launcher.Properties.Resources.org_logo;
            this.pictureBox1.Location = new System.Drawing.Point(304, 68);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(69, 69);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.ToolTip_Main.SetToolTip(this.pictureBox1, "AUM is made free and open-source by BitCrackers\r\nhttps://github.com/BitCrackers");
            // 
            // chboxUseDebugBuild
            // 
            this.chboxUseDebugBuild.AutoSize = true;
            this.chboxUseDebugBuild.Location = new System.Drawing.Point(5, 107);
            this.chboxUseDebugBuild.Name = "chboxUseDebugBuild";
            this.chboxUseDebugBuild.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chboxUseDebugBuild.Size = new System.Drawing.Size(103, 17);
            this.chboxUseDebugBuild.TabIndex = 7;
            this.chboxUseDebugBuild.Text = "Use debug build";
            this.chboxUseDebugBuild.UseVisualStyleBackColor = true;
            this.chboxUseDebugBuild.CheckedChanged += new System.EventHandler(this.chboxUseDebugBuild_CheckedChanged);
            // 
            // btnSwitchToInjectable
            // 
            this.btnSwitchToInjectable.Location = new System.Drawing.Point(115, 82);
            this.btnSwitchToInjectable.Name = "btnSwitchToInjectable";
            this.btnSwitchToInjectable.Size = new System.Drawing.Size(126, 20);
            this.btnSwitchToInjectable.TabIndex = 6;
            this.btnSwitchToInjectable.Text = "Switch to Injectable";
            this.btnSwitchToInjectable.UseVisualStyleBackColor = true;
            this.btnSwitchToInjectable.Click += new System.EventHandler(this.btnSwitchToInjectable_Click);
            // 
            // btnSwitchToProxy
            // 
            this.btnSwitchToProxy.Enabled = false;
            this.btnSwitchToProxy.Location = new System.Drawing.Point(5, 82);
            this.btnSwitchToProxy.Name = "btnSwitchToProxy";
            this.btnSwitchToProxy.Size = new System.Drawing.Size(105, 20);
            this.btnSwitchToProxy.TabIndex = 5;
            this.btnSwitchToProxy.Text = "Switch to Proxy";
            this.btnSwitchToProxy.UseVisualStyleBackColor = true;
            this.btnSwitchToProxy.Click += new System.EventHandler(this.btnSwitchToProxy_Click);
            // 
            // lblCurrentBuild
            // 
            this.lblCurrentBuild.AutoSize = true;
            this.lblCurrentBuild.Location = new System.Drawing.Point(5, 67);
            this.lblCurrentBuild.Name = "lblCurrentBuild";
            this.lblCurrentBuild.Size = new System.Drawing.Size(112, 13);
            this.lblCurrentBuild.TabIndex = 4;
            this.lblCurrentBuild.Text = "Currently using: (none)";
            // 
            // btnUpdateLauncher
            // 
            this.btnUpdateLauncher.Enabled = false;
            this.btnUpdateLauncher.Location = new System.Drawing.Point(309, 12);
            this.btnUpdateLauncher.Name = "btnUpdateLauncher";
            this.btnUpdateLauncher.Size = new System.Drawing.Size(64, 22);
            this.btnUpdateLauncher.TabIndex = 2;
            this.btnUpdateLauncher.Text = "Update?";
            this.btnUpdateLauncher.UseVisualStyleBackColor = true;
            this.btnUpdateLauncher.Click += new System.EventHandler(this.btnUpdateLauncher_Click);
            // 
            // lblLauncherVersion
            // 
            this.lblLauncherVersion.AutoSize = true;
            this.lblLauncherVersion.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblLauncherVersion.Location = new System.Drawing.Point(5, 16);
            this.lblLauncherVersion.Name = "lblLauncherVersion";
            this.lblLauncherVersion.Size = new System.Drawing.Size(126, 13);
            this.lblLauncherVersion.TabIndex = 0;
            this.lblLauncherVersion.Text = "Launcher Version: (none)";
            // 
            // StatusStrip_Main
            // 
            this.StatusStrip_Main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLbl_Launcher,
            this.StatusLbl_AUM,
            this.StatusLbl_Game,
            this.StatusLbl_Injection});
            this.StatusStrip_Main.Location = new System.Drawing.Point(0, 411);
            this.StatusStrip_Main.Name = "StatusStrip_Main";
            this.StatusStrip_Main.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.StatusStrip_Main.Size = new System.Drawing.Size(399, 22);
            this.StatusStrip_Main.SizingGrip = false;
            this.StatusStrip_Main.TabIndex = 5;
            this.StatusStrip_Main.Text = "statusStrip1";
            // 
            // StatusLbl_Launcher
            // 
            this.StatusLbl_Launcher.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StatusLbl_Launcher.Name = "StatusLbl_Launcher";
            this.StatusLbl_Launcher.Size = new System.Drawing.Size(79, 17);
            this.StatusLbl_Launcher.Text = "Launcher: init";
            // 
            // StatusLbl_AUM
            // 
            this.StatusLbl_AUM.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StatusLbl_AUM.Name = "StatusLbl_AUM";
            this.StatusLbl_AUM.Size = new System.Drawing.Size(57, 17);
            this.StatusLbl_AUM.Text = "AUM: init";
            // 
            // StatusLbl_Game
            // 
            this.StatusLbl_Game.ForeColor = System.Drawing.Color.LimeGreen;
            this.StatusLbl_Game.Name = "StatusLbl_Game";
            this.StatusLbl_Game.Size = new System.Drawing.Size(60, 17);
            this.StatusLbl_Game.Text = "Game: OK";
            // 
            // StatusLbl_Injection
            // 
            this.StatusLbl_Injection.ForeColor = System.Drawing.Color.DarkOrange;
            this.StatusLbl_Injection.Name = "StatusLbl_Injection";
            this.StatusLbl_Injection.Size = new System.Drawing.Size(107, 17);
            this.StatusLbl_Injection.Text = "Injection: waiting...";
            // 
            // rtxtLog
            // 
            this.rtxtLog.HideSelection = false;
            this.rtxtLog.Location = new System.Drawing.Point(10, 227);
            this.rtxtLog.Name = "rtxtLog";
            this.rtxtLog.ReadOnly = true;
            this.rtxtLog.Size = new System.Drawing.Size(379, 181);
            this.rtxtLog.TabIndex = 7;
            this.rtxtLog.Text = "";
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Locate Among Us Install Location ...";
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // ProcessPollWorker
            // 
            this.ProcessPollWorker.WorkerSupportsCancellation = true;
            this.ProcessPollWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ProcessPollWorker_DoWork);
            this.ProcessPollWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ProcessPollWorker_RunWorkerCompleted);
            // 
            // HashWorker
            // 
            this.HashWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.HashWorker_DoWork);
            this.HashWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.HashWorker_RunWorkerCompleted);
            // 
            // GithubWorker
            // 
            this.GithubWorker.WorkerReportsProgress = true;
            this.GithubWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.GithubWorker_DoWork);
            this.GithubWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.GithubWorker_ProgressChanged);
            this.GithubWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.GithubWorker_RunWorkerCompleted);
            // 
            // GameDirWorker
            // 
            this.GameDirWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.GameDirWorker_DoWork);
            this.GameDirWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.GameDirWorker_RunWorkerCompleted);
            // 
            // DownloadWorker
            // 
            this.DownloadWorker.WorkerReportsProgress = true;
            this.DownloadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DownloadWorker_DoWork);
            this.DownloadWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.DownloadWorker_RunWorkerCompleted);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 433);
            this.Controls.Add(this.rtxtLog);
            this.Controls.Add(this.StatusStrip_Main);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblGameDetails);
            this.Controls.Add(this.lblProcessDetails);
            this.Controls.Add(this.pictureBoxProcessIcon);
            this.Controls.Add(this.MenuStrip_Main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MenuStrip_Main;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "AmongUsMenu Launcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.MenuStrip_Main.ResumeLayout(false);
            this.MenuStrip_Main.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProcessIcon)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.StatusStrip_Main.ResumeLayout(false);
            this.StatusStrip_Main.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MenuStrip_Main;
        private System.Windows.Forms.ToolStripMenuItem Menu_MenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem MenuAbout_MenuItem;
        private System.Windows.Forms.PictureBox pictureBoxProcessIcon;
        private System.Windows.Forms.Label lblProcessDetails;
        private System.Windows.Forms.Label lblGameDetails;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chboxUseDebugBuild;
        private System.Windows.Forms.Button btnSwitchToInjectable;
        private System.Windows.Forms.Button btnSwitchToProxy;
        private System.Windows.Forms.Label lblCurrentBuild;
        private System.Windows.Forms.Button btnUpdateLauncher;
        private System.Windows.Forms.Label lblLauncherVersion;
        private System.Windows.Forms.StatusStrip StatusStrip_Main;
        private System.Windows.Forms.ToolStripStatusLabel StatusLbl_Launcher;
        private System.Windows.Forms.ToolStripStatusLabel StatusLbl_AUM;
        private System.Windows.Forms.ToolStripStatusLabel StatusLbl_Game;
        private System.Windows.Forms.ToolStripStatusLabel StatusLbl_Injection;
        private System.Windows.Forms.Button btnToggleLogDisplay;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolTip ToolTip_Main;
        private System.Windows.Forms.RichTextBox rtxtLog;
        private System.Windows.Forms.ComboBox cboxTaggedVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.ComponentModel.BackgroundWorker ProcessPollWorker;
        private System.ComponentModel.BackgroundWorker HashWorker;
        private System.ComponentModel.BackgroundWorker GithubWorker;
        private System.ComponentModel.BackgroundWorker GameDirWorker;
        private System.ComponentModel.BackgroundWorker DownloadWorker;
        private System.Windows.Forms.Button btnInject;
    }
}

