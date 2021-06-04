using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.ComponentModel;

namespace aum_launcher
{
    public partial class Main
    {
        public class ProcessInfoItem
        {
            public Process ActiveProcess = null;
            public int PID = -1;
            public string Name = "";
            public string FilePath = "";
            public Icon Icon = null;
        }

        private ProcessInfoItem SelectedProcess;

        public void SetWatchedProcess(string processFileName)
        {
            SelectedProcess = new ProcessInfoItem();
            SelectedProcess.FilePath = processFileName;
            Logger.Log.Write("ProcessWatcher set to watch for '" + processFileName + "'", Logger.ELogType.Info, rtxtLog);
            if (File.Exists(processFileName))
            {
                SelectedProcess.Icon = Icon.ExtractAssociatedIcon(processFileName);
                pictureBoxProcessIcon.Image = SelectedProcess.Icon.ToBitmap();
                Logger.Log.Write("ProcessWatcher loaded process icon", Logger.ELogType.Info, rtxtLog);
            }
            LostActiveProcess();
        }

        private void SetActiveProcess(Process proc)
        {
            SelectedProcess.ActiveProcess = proc;
            SelectedProcess.PID = SelectedProcess.ActiveProcess.Id;
            lblProcessDetails.ForeColor = System.Drawing.Color.Black;
            lblProcessDetails.Text = "Process: " + SelectedProcess.ActiveProcess.ProcessName + "(PID: " + SelectedProcess.PID.ToString() + ")";

            Logger.Log.Write("ProcessWatcher found game process '" + SelectedProcess.ActiveProcess.ProcessName + "' with PID " + SelectedProcess.PID.ToString(), Logger.ELogType.Info, rtxtLog);

            if (SelectedProcess.Name == "")
            {
                SelectedProcess.Name = SelectedProcess.ActiveProcess.ProcessName;
            }
            try
            {
                SelectedProcess.ActiveProcess.EnableRaisingEvents = true;
                SelectedProcess.ActiveProcess.SynchronizingObject = this;
                SelectedProcess.ActiveProcess.Exited += OnProcessExited;
            }
            catch (Exception ex)
            {
                Logger.Log.Alert("Could not subscribe to process' Exited event. Cannot auto-detect when selected process closes or becomes invalid.\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void LostActiveProcess()
        {
            lblProcessDetails.Text = "Waiting for game...";
            Logger.Log.Write("ProcessWatcher lost game process, beginning search again...", Logger.ELogType.Notification, rtxtLog);
            StatusLbl_Injection.ForeColor = System.Drawing.Color.DarkOrange;
            StatusLbl_Injection.Text = "waiting...";
            // check if background worker is busy. if not, give it its task
            // if it is busy, welllll.... why would it be busy?
            if (ProcessPollWorker.IsBusy)
            {
                ProcessPollWorker.CancelAsync();
                Logger.Log.Write("ProcessWatcher's worker was busy, spinning until free...", Logger.ELogType.Info, rtxtLog);
                while (ProcessPollWorker.IsBusy)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            // poll continually for new processes matching the old one's filepath
            ProcessPollWorker.RunWorkerAsync(SelectedProcess.FilePath);
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            LostActiveProcess();
        }

        private void ProcessPollWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // TO-DO:
            // better error handling
            // timeout functionality
            string processFilePath = e.Argument as string;
            Logger.Log.Write("ProcessWatcher's worker beginning scan for '" + processFilePath + "'", Logger.ELogType.Info, rtxtLog);
            Process[] allProcesses;
            while (true)
            {
                if (ProcessPollWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                allProcesses = Process.GetProcesses();
                foreach (Process proc in allProcesses)
                {
                    try
                    {
                        if (proc.HasExited) continue;

                        ProcessModule procMainModule = proc.MainModule;
                        if (procMainModule.FileName == processFilePath)
                        {
                            e.Result = proc;
                            return;
                        }
                    }
                    // NOTE:
                    // this exception is thrown when the process is a protected process.
                    // harmless in our case
                    catch (Win32Exception w32e) { }
                    // TO-DO:
                    // error handling
                    catch (Exception ex) { }
                }
                System.Threading.Thread.Sleep(500);
            }
        }

        private void ProcessPollWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Error != null) || (e.Cancelled))
            {
                // TO-DO:
                // handle this
                Logger.Log.Write("ProcessWatcher's worker failed or was cancelled", Logger.ELogType.Notification, rtxtLog);
                return;
            }
            if (e.Result != null)
            {
                SetActiveProcess(e.Result as Process);
            }
        }
    }
}
