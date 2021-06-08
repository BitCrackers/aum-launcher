using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace aum_launcher
{
    public class Logger
    {
        const string LOG_FILENAME = Main.LAUNCHER_NAME + "_log.txt";

        public static readonly Logger Log = new Logger();
        static readonly System.Globalization.CultureInfo DateTimeCultureInfo_German = System.Globalization.CultureInfo.CreateSpecificCulture("de-DE");

        // mostly used for coloring messages
        public enum ELogType
        {
            Info, // black
            Notification, // blue
            Error, // red
            Exception // red, then black for stacktrace
        }

        // TO-DO:
        // * Is StreamWriter thread-safe?
        StreamWriter logWriter;

        Logger()
        {
            try
            {
                logWriter = new StreamWriter(LOG_FILENAME, true) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Could not initialize Logger: " + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        // NOTE:
        // this is actually pretty bad. C# destructors should only be used to clean up unmanaged resources under unexpected conditions.
        // but i guess the underlying file handle counts as an unmanaged resource, right?
        ~Logger()
        {
            Close();
        }

        public void Close()
        {
            if (logWriter != null)
            {
                logWriter.Flush();
                logWriter.Close();
            }
        }

        /*
        public void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            this.UnityLog("====LOG START====\n(" + type.ToString() + "): " + logString + "\nStack Trace: \n" + stackTrace + "\n====LOG END====");
        }
        */

        public void Write(string logString, ELogType type, System.Windows.Forms.RichTextBox rtxtLog = null, bool shouldForceFlush = false)
        {
            logString = "[" + DateTime.Now.ToString("G", DateTimeCultureInfo_German) + "]: " + logString + "\n";
            if (logWriter != null)
            {
                if (logWriter.BaseStream.CanWrite)
                {
                    logWriter.WriteLine(logString);
                }
                if (shouldForceFlush)
                    logWriter.Flush();
            }

            if (rtxtLog != null)
            {
                WinformControl_WriteTextSafe(rtxtLog, logString);
            }
        }

        public void WriteError(string logString, System.Windows.Forms.RichTextBox rtxtLog = null, bool shouldForceFlush = false)
        {
            Write(logString, ELogType.Error, rtxtLog, shouldForceFlush);
        }

        public void Alert(string logString, string caption, System.Windows.Forms.MessageBoxButtons buttons, System.Windows.Forms.MessageBoxIcon icon, ELogType type = ELogType.Info, System.Windows.Forms.RichTextBox rtxtLog = null, bool shouldForceFlush = false)
        {
            System.Windows.Forms.MessageBox.Show(logString, caption, buttons, icon);
            Write(logString, type, rtxtLog, shouldForceFlush);
        }

        private delegate void SafeCallDelegate(System.Windows.Forms.RichTextBox richtextControl, string text);
        private void WinformControl_WriteTextSafe(System.Windows.Forms.RichTextBox richtextControl, string text)
        {
            if (richtextControl.InvokeRequired)
            {
                var d = new SafeCallDelegate(WinformControl_WriteTextSafe);
                richtextControl.Invoke(d, new object[] { richtextControl, text });
            }
            else
            {
                richtextControl.AppendText(text);
            }
        }
    }
}
