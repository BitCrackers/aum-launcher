using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace aum_launcher
{
    public static class ProcessUtils
    {
        public static IntPtr GetProcessBase(IntPtr procHandle)
        {
            int capacity = 260;
            StringBuilder sb = new StringBuilder(capacity);
            bool success = WinAPI.QueryFullProcessImageName(procHandle, 0, sb, ref capacity);
            Logger.Log.Write("QueryFullProcessImageName returned: " + success.ToString(), Logger.ELogType.Info);
            if (success == false)
            {
                Logger.Log.WriteError("QueryFullProcessImageName failed, cannot continue.");
                return IntPtr.Zero;
            }
            string fullPath = sb.ToString(0, capacity);
            Logger.Log.Write("QueryFullProcessImageName got '" + fullPath + "' from process", Logger.ELogType.Info);

            // QueryFullProcessImageName returns the full dos path, but we only want the filename part, so strip out the rest
            string moduleName = Path.GetFileName(fullPath);

            return GetModuleBase(procHandle, moduleName);
        }

        public static IntPtr GetModuleBase(IntPtr procHandle, string targetModuleName)
        {
            targetModuleName = targetModuleName.ToLower();
            IntPtr baseAddr = IntPtr.Zero;

            // Setting up the variable for the second argument for EnumProcessModules
            IntPtr[] hMods = new IntPtr[1024];

            GCHandle gch = GCHandle.Alloc(hMods, GCHandleType.Pinned); // Don't forget to free this later
            IntPtr pModules = gch.AddrOfPinnedObject();

            // Setting up the rest of the parameters for EnumProcessModules
            uint uiSize = (uint)(Marshal.SizeOf(typeof(IntPtr)) * (hMods.Length));
            uint cbNeeded = 0;

            bool enumProcessResult = WinAPI.EnumProcessModulesEx(procHandle, pModules, uiSize, out cbNeeded, (uint)(WinAPI.EnumProcessModulesFilterFlag.LIST_MODULES_ALL));
            Logger.Log.Write("EnumProcessModulesEx returned: " + enumProcessResult.ToString(), Logger.ELogType.Info);
            if (enumProcessResult == false)
            {
                Logger.Log.WriteError("EnumProcessModulesEx failed, cannot continue");
                return IntPtr.Zero;
            }

            Int32 uiTotalNumberofModules = (Int32)(cbNeeded / (Marshal.SizeOf(typeof(IntPtr))));
            Logger.Log.Write("Number of Modules: " + uiTotalNumberofModules, Logger.ELogType.Info);

            for (int i = 0; i < (int)uiTotalNumberofModules; i++)
            {
                StringBuilder strbld = new StringBuilder(260);

                uint size = WinAPI.GetModuleFileNameEx(procHandle, hMods[i], strbld, (uint)(strbld.Capacity));
                if (size == 0)
                {
                    Logger.Log.WriteError("GetModuleFileNameEx failed, cannot continue");
                    gch.Free();
                    return IntPtr.Zero;
                }
                string curModuleFilePath = strbld.ToString();
                Logger.Log.Write("File Path: " + curModuleFilePath, Logger.ELogType.Info);
                Logger.Log.Write("Base Addr: " + hMods[i].ToString("X2"), Logger.ELogType.Info);

                // GetModuleFileNameEx returns the full dos path, we're just interested in the filename part though, so strip all the other stuff out...
                string curModuleFileName = Path.GetFileName(curModuleFilePath).ToLower();
                if (curModuleFileName == targetModuleName)
                {
                    baseAddr = hMods[i];
                    Logger.Log.Write("Found match! Base address of process should be " + hMods[i].ToString("X2"), Logger.ELogType.Info);
                    break;
                }
            }

            // Must free the GCHandle object
            gch.Free();

            return baseAddr;
        }
    }
}
