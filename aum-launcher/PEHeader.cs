using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace aum_launcher
{
    #region Defines

    public enum EProcessAddressSpace
    {
        x86,
        x64
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DOS_HEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public char[] e_magic;       // Magic number
        public UInt16 e_cblp;    // Bytes on last page of file
        public UInt16 e_cp;      // Pages in file
        public UInt16 e_crlc;    // Relocations
        public UInt16 e_cparhdr;     // Size of header in paragraphs
        public UInt16 e_minalloc;    // Minimum extra paragraphs needed
        public UInt16 e_maxalloc;    // Maximum extra paragraphs needed
        public UInt16 e_ss;      // Initial (relative) SS value
        public UInt16 e_sp;      // Initial SP value
        public UInt16 e_csum;    // Checksum
        public UInt16 e_ip;      // Initial IP value
        public UInt16 e_cs;      // Initial (relative) CS value
        public UInt16 e_lfarlc;      // File address of relocation table
        public UInt16 e_ovno;    // Overlay number
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt16[] e_res1;    // Reserved words
        public UInt16 e_oemid;       // OEM identifier (for e_oeminfo)
        public UInt16 e_oeminfo;     // OEM information; e_oemid specific
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt16[] e_res2;    // Reserved words
        public Int32 e_lfanew;      // File address of new exe header

        private string _e_magic
        {
            get { return new string(e_magic); }
        }

        public bool isValid
        {
            get { return _e_magic == "MZ"; }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_NT_HEADERS32
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Signature;

        [FieldOffset(4)]
        public IMAGE_FILE_HEADER FileHeader;

        [FieldOffset(24)]
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader;

        private string _Signature
        {
            get { return new string(Signature); }
        }

        public bool isValid
        {
            get { return _Signature == "PE\0\0" && OptionalHeader.Magic == MagicType.IMAGE_NT_OPTIONAL_HDR32_MAGIC; }
        }
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_NT_HEADERS64
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Signature;

        [FieldOffset(4)]
        public IMAGE_FILE_HEADER FileHeader;

        [FieldOffset(24)]
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader;

        private string _Signature
        {
            get { return new string(Signature); }
        }

        public bool isValid
        {
            get { return _Signature == "PE\0\0" && OptionalHeader.Magic == MagicType.IMAGE_NT_OPTIONAL_HDR64_MAGIC; }
        }
    }

    public enum MachineType : ushort
    {
        Native = 0,
        I386 = 0x014c,
        Itanium = 0x0200,
        x64 = 0x8664
    }
    public enum MagicType : ushort
    {
        IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
        IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b
    }
    public enum SubSystemType : ushort
    {
        IMAGE_SUBSYSTEM_UNKNOWN = 0,
        IMAGE_SUBSYSTEM_NATIVE = 1,
        IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,
        IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,
        IMAGE_SUBSYSTEM_POSIX_CUI = 7,
        IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9,
        IMAGE_SUBSYSTEM_EFI_APPLICATION = 10,
        IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
        IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12,
        IMAGE_SUBSYSTEM_EFI_ROM = 13,
        IMAGE_SUBSYSTEM_XBOX = 14

    }
    public enum DllCharacteristicsType : ushort
    {
        RES_0 = 0x0001,
        RES_1 = 0x0002,
        RES_2 = 0x0004,
        RES_3 = 0x0008,
        IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE = 0x0040,
        IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY = 0x0080,
        IMAGE_DLL_CHARACTERISTICS_NX_COMPAT = 0x0100,
        IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,
        IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,
        IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,
        RES_4 = 0x1000,
        IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,
        IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_OPTIONAL_HEADER32
    {
        [FieldOffset(0)]
        public MagicType Magic;

        [FieldOffset(2)]
        public byte MajorLinkerVersion;

        [FieldOffset(3)]
        public byte MinorLinkerVersion;

        [FieldOffset(4)]
        public uint SizeOfCode;

        [FieldOffset(8)]
        public uint SizeOfInitializedData;

        [FieldOffset(12)]
        public uint SizeOfUninitializedData;

        [FieldOffset(16)]
        public uint AddressOfEntryPoint;

        [FieldOffset(20)]
        public uint BaseOfCode;

        // PE32 contains this additional field
        [FieldOffset(24)]
        public uint BaseOfData;

        [FieldOffset(28)]
        public uint ImageBase;

        [FieldOffset(32)]
        public uint SectionAlignment;

        [FieldOffset(36)]
        public uint FileAlignment;

        [FieldOffset(40)]
        public ushort MajorOperatingSystemVersion;

        [FieldOffset(42)]
        public ushort MinorOperatingSystemVersion;

        [FieldOffset(44)]
        public ushort MajorImageVersion;

        [FieldOffset(46)]
        public ushort MinorImageVersion;

        [FieldOffset(48)]
        public ushort MajorSubsystemVersion;

        [FieldOffset(50)]
        public ushort MinorSubsystemVersion;

        [FieldOffset(52)]
        public uint Win32VersionValue;

        [FieldOffset(56)]
        public uint SizeOfImage;

        [FieldOffset(60)]
        public uint SizeOfHeaders;

        [FieldOffset(64)]
        public uint CheckSum;

        [FieldOffset(68)]
        public SubSystemType Subsystem;

        [FieldOffset(70)]
        public DllCharacteristicsType DllCharacteristics;

        [FieldOffset(72)]
        public uint SizeOfStackReserve;

        [FieldOffset(76)]
        public uint SizeOfStackCommit;

        [FieldOffset(80)]
        public uint SizeOfHeapReserve;

        [FieldOffset(84)]
        public uint SizeOfHeapCommit;

        [FieldOffset(88)]
        public uint LoaderFlags;

        [FieldOffset(92)]
        public uint NumberOfRvaAndSizes;

        [FieldOffset(96)]
        public IMAGE_DATA_DIRECTORY ExportTable;

        [FieldOffset(104)]
        public IMAGE_DATA_DIRECTORY ImportTable;

        [FieldOffset(112)]
        public IMAGE_DATA_DIRECTORY ResourceTable;

        [FieldOffset(120)]
        public IMAGE_DATA_DIRECTORY ExceptionTable;

        [FieldOffset(128)]
        public IMAGE_DATA_DIRECTORY CertificateTable;

        [FieldOffset(136)]
        public IMAGE_DATA_DIRECTORY BaseRelocationTable;

        [FieldOffset(144)]
        public IMAGE_DATA_DIRECTORY Debug;

        [FieldOffset(152)]
        public IMAGE_DATA_DIRECTORY Architecture;

        [FieldOffset(160)]
        public IMAGE_DATA_DIRECTORY GlobalPtr;

        [FieldOffset(168)]
        public IMAGE_DATA_DIRECTORY TLSTable;

        [FieldOffset(176)]
        public IMAGE_DATA_DIRECTORY LoadConfigTable;

        [FieldOffset(184)]
        public IMAGE_DATA_DIRECTORY BoundImport;

        [FieldOffset(192)]
        public IMAGE_DATA_DIRECTORY IAT;

        [FieldOffset(200)]
        public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

        [FieldOffset(208)]
        public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

        [FieldOffset(216)]
        public IMAGE_DATA_DIRECTORY Reserved;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_OPTIONAL_HEADER64
    {
        [FieldOffset(0)]
        public MagicType Magic;

        [FieldOffset(2)]
        public byte MajorLinkerVersion;

        [FieldOffset(3)]
        public byte MinorLinkerVersion;

        [FieldOffset(4)]
        public uint SizeOfCode;

        [FieldOffset(8)]
        public uint SizeOfInitializedData;

        [FieldOffset(12)]
        public uint SizeOfUninitializedData;

        [FieldOffset(16)]
        public uint AddressOfEntryPoint;

        [FieldOffset(20)]
        public uint BaseOfCode;

        [FieldOffset(24)]
        public ulong ImageBase;

        [FieldOffset(32)]
        public uint SectionAlignment;

        [FieldOffset(36)]
        public uint FileAlignment;

        [FieldOffset(40)]
        public ushort MajorOperatingSystemVersion;

        [FieldOffset(42)]
        public ushort MinorOperatingSystemVersion;

        [FieldOffset(44)]
        public ushort MajorImageVersion;

        [FieldOffset(46)]
        public ushort MinorImageVersion;

        [FieldOffset(48)]
        public ushort MajorSubsystemVersion;

        [FieldOffset(50)]
        public ushort MinorSubsystemVersion;

        [FieldOffset(52)]
        public uint Win32VersionValue;

        [FieldOffset(56)]
        public uint SizeOfImage;

        [FieldOffset(60)]
        public uint SizeOfHeaders;

        [FieldOffset(64)]
        public uint CheckSum;

        [FieldOffset(68)]
        public SubSystemType Subsystem;

        [FieldOffset(70)]
        public DllCharacteristicsType DllCharacteristics;

        [FieldOffset(72)]
        public ulong SizeOfStackReserve;

        [FieldOffset(80)]
        public ulong SizeOfStackCommit;

        [FieldOffset(88)]
        public ulong SizeOfHeapReserve;

        [FieldOffset(96)]
        public ulong SizeOfHeapCommit;

        [FieldOffset(104)]
        public uint LoaderFlags;

        [FieldOffset(108)]
        public uint NumberOfRvaAndSizes;

        [FieldOffset(112)]
        public IMAGE_DATA_DIRECTORY ExportTable;

        [FieldOffset(120)]
        public IMAGE_DATA_DIRECTORY ImportTable;

        [FieldOffset(128)]
        public IMAGE_DATA_DIRECTORY ResourceTable;

        [FieldOffset(136)]
        public IMAGE_DATA_DIRECTORY ExceptionTable;

        [FieldOffset(144)]
        public IMAGE_DATA_DIRECTORY CertificateTable;

        [FieldOffset(152)]
        public IMAGE_DATA_DIRECTORY BaseRelocationTable;

        [FieldOffset(160)]
        public IMAGE_DATA_DIRECTORY Debug;

        [FieldOffset(168)]
        public IMAGE_DATA_DIRECTORY Architecture;

        [FieldOffset(176)]
        public IMAGE_DATA_DIRECTORY GlobalPtr;

        [FieldOffset(184)]
        public IMAGE_DATA_DIRECTORY TLSTable;

        [FieldOffset(192)]
        public IMAGE_DATA_DIRECTORY LoadConfigTable;

        [FieldOffset(200)]
        public IMAGE_DATA_DIRECTORY BoundImport;

        [FieldOffset(208)]
        public IMAGE_DATA_DIRECTORY IAT;

        [FieldOffset(216)]
        public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

        [FieldOffset(224)]
        public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

        [FieldOffset(232)]
        public IMAGE_DATA_DIRECTORY Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DATA_DIRECTORY
    {
        public UInt32 VirtualAddress;
        public UInt32 Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_FILE_HEADER
    {
        public UInt16 Machine;
        public UInt16 NumberOfSections;
        public UInt32 TimeDateStamp;
        public UInt32 PointerToSymbolTable;
        public UInt32 NumberOfSymbols;
        public UInt16 SizeOfOptionalHeader;
        public UInt16 Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_EXPORT_DIRECTORY
    {
        public UInt32 Characteristics;
        public UInt32 TimeDateStamp;
        public UInt16 MajorVersion;
        public UInt16 MinorVersion;
        public UInt32 Name;
        public UInt32 Base;
        public UInt32 NumberOfFunctions;
        public UInt32 NumberOfNames;
        public UInt32 AddressOfFunctions;     // RVA from base of image
        public UInt32 AddressOfNames;     // RVA from base of image
        public UInt32 AddressOfNameOrdinals;  // RVA from base of image
    }
    #endregion

    public class PEHeader
    {
        IntPtr baseAddress = IntPtr.Zero;

        IMAGE_DOS_HEADER dosHeader;
        IMAGE_NT_HEADERS64 ntHeader64;
        IMAGE_NT_HEADERS32 ntHeader32;

        bool hasEAT = false;
        IMAGE_EXPORT_DIRECTORY EAT;
        uint[] EAT_SymbolRVAs;
        uint[] EAT_NameRVAs;
        ushort[] EAT_Ordinals;
        // massive, massive array. like 40 KB.
        byte[] EAT_NameHeap;
        // processed data for quicker searches
        Dictionary<uint, string> EAT_ProcessedNames;

        public bool Is64Bit = false;
        public bool IsValid = false;


        private PEHeader() { }

        public IntPtr GetAddressOfExportedFunction(Process proc, string funcName)
        {
            if (IsValid == false) return IntPtr.Zero;
            IntPtr procHandle = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, 0, (uint)proc.Id);

            if (hasEAT == false)
            {
                if (ReadEAT(procHandle) == false)
                {
                    Logger.Log.WriteError("Could not read EAT. Cannot find any exported functions.");
                    WinAPI.CloseHandle(procHandle);
                    return IntPtr.Zero;
                }
            }
            // begin scanning
            // search names heap for matching function name.
            foreach (KeyValuePair<uint, string> nameEntry in EAT_ProcessedNames)
            {
                if (nameEntry.Value == funcName)
                {
                    Console.WriteLine("Found target func '" + nameEntry.Value + "' at index " + nameEntry.Key.ToString() + " in name/ordinal array");
                    uint indexIntoFuncArray = EAT_Ordinals[nameEntry.Key];
                    uint funcRVA = EAT_SymbolRVAs[indexIntoFuncArray];
                    IntPtr funcAddress = baseAddress + (int)funcRVA;
                    Logger.Log.Write("Found function address @ " + funcAddress.ToString("X2") + " for '" + funcName + "'", Logger.ELogType.Info);
                    WinAPI.CloseHandle(procHandle);
                    return funcAddress;
                }
            }
            Logger.Log.WriteError("Could not find exported function '" + funcName + "' from PEHeader at " + baseAddress.ToString("X2"));
            WinAPI.CloseHandle(procHandle);
            return IntPtr.Zero;
        }

        bool ReadEAT(IntPtr procHandle)
        {
            int numBytesRead = 0;
            IntPtr numBytesReadAsIntPtr = IntPtr.Zero;
            IntPtr eatAddr = baseAddress;
            uint eatSize = 0;
            if (Is64Bit)
            {
                eatAddr += (int)ntHeader64.OptionalHeader.ExportTable.VirtualAddress;
                eatSize = ntHeader64.OptionalHeader.ExportTable.Size;
            }
            else
            {
                eatAddr += (int)ntHeader32.OptionalHeader.ExportTable.VirtualAddress;
                eatSize = ntHeader32.OptionalHeader.ExportTable.Size;
            }
            EAT = WinAPI.Rpm<IMAGE_EXPORT_DIRECTORY>(procHandle, eatAddr, out numBytesRead);
            if (numBytesRead == 0)
            {
                Logger.Log.WriteError("Could not read EAT from " + eatAddr.ToString("X2"));
                hasEAT = false;
                return false;
            }
            EAT_SymbolRVAs = new uint[EAT.NumberOfFunctions];
            EAT_NameRVAs = new uint[EAT.NumberOfNames];
            EAT_Ordinals = new ushort[EAT.NumberOfNames];
            // the heap may contain forward export names too, so we want to allocate a little more to account for them
            EAT_ProcessedNames = new Dictionary<uint, string>((int)EAT.NumberOfNames);

            Logger.Log.Write("Read EAT at " + eatAddr.ToString("X2") + "\nnumBytesRead: " + numBytesRead.ToString(), Logger.ELogType.Info);

            // begin reading...
            // note: pointer sizes vary based on target bitness! complicates things a bit
            // 1. AddressOfFunctions, an array of int32 RVAs that when added to image base lead to the exported symbol. some entries may be forwarder rvas that lead to a string.
            WinAPI.ReadProcessMemory(procHandle, (IntPtr)(baseAddress + (int)EAT.AddressOfFunctions), EAT_SymbolRVAs, (IntPtr)(EAT.NumberOfFunctions * 4), out numBytesReadAsIntPtr);
            Logger.Log.Write("Read " + EAT.NumberOfFunctions + " function RVAs starting at " + ((IntPtr)(baseAddress + (int)EAT.AddressOfFunctions)).ToString() + " and ending at " + ((IntPtr)(baseAddress + (int)EAT.AddressOfFunctions) + (int)(EAT.NumberOfFunctions * 4)).ToString(), Logger.ELogType.Info);
            // 2. AddressOfNames, an array of int32 RVAs to c-strings (we have to read the c-strings too!)
            WinAPI.ReadProcessMemory(procHandle, (IntPtr)(baseAddress + (int)EAT.AddressOfNames), EAT_NameRVAs, (IntPtr)(EAT.NumberOfNames * 4), out numBytesReadAsIntPtr);
            Logger.Log.Write("Read " + EAT.NumberOfNames + " name RVAs starting at " + ((IntPtr)(baseAddress + (int)EAT.AddressOfNames)).ToString("X2") + " and ending at " + ((IntPtr)(baseAddress + (int)EAT.AddressOfNames) + (int)(EAT.NumberOfNames * 4)).ToString(), Logger.ELogType.Info);
            // 3. AddressOfNameOrdinals, an array of ushorts mirroring AddressOfNames, each ushort is the index into AddressOfFunctions
            WinAPI.ReadProcessMemory(procHandle, (IntPtr)(baseAddress + (int)EAT.AddressOfNameOrdinals), EAT_Ordinals, (IntPtr)(EAT.NumberOfNames * 2), out numBytesReadAsIntPtr);
            Logger.Log.Write("Read " + EAT.NumberOfNames + " name RVAs starting at " + ((IntPtr)(baseAddress + (int)EAT.AddressOfNames)).ToString("X2") + " and ending at " + ((IntPtr)(baseAddress + (int)EAT.AddressOfNames) + (int)(EAT.NumberOfNames * 4)).ToString(), Logger.ELogType.Info);

            // 4. we want to avoid making 1700 ReadProcessMemory calls, so we should try reading as many of the names as we can in one call
            // we do this by going through the name RVAs and finding both the lowest RVA and the highest RVA and make our one ReadProcessMemory in that range.
            // this is a really, really bad approach but it's the one i'm doing because i'm lazy
            // the better approach is to implement a search algorithm that starts at both ends of the RVA array, and then divides it in half, then divides those halves in half, etc, until we get close to our search target
            uint lowestNameRVA = uint.MaxValue;
            uint lowestNameRVAIndex = uint.MaxValue;
            uint highestNameRVA = 0;
            uint highestNameRVAIndex = uint.MaxValue;

            int sizeOfNameHeap = 0;
            uint curIndex = 0;
            foreach (uint nameRVA in EAT_NameRVAs)
            {
                if (nameRVA > highestNameRVA)
                {
                    highestNameRVA = nameRVA;
                    highestNameRVAIndex = curIndex;
                }
                if (nameRVA < lowestNameRVA)
                {
                    lowestNameRVA = nameRVA;
                    lowestNameRVAIndex = curIndex;
                }
                curIndex++;
            }
            sizeOfNameHeap = (int)(highestNameRVA - lowestNameRVA);
            Logger.Log.Write("Saw lowestNameRVA of " + lowestNameRVA.ToString() + " at index " + lowestNameRVAIndex.ToString(), Logger.ELogType.Info);
            Logger.Log.Write("Saw highestNameRVA of " + highestNameRVA.ToString() + " at index " + highestNameRVAIndex.ToString(), Logger.ELogType.Info);

            EAT_NameHeap = new byte[sizeOfNameHeap];
            Logger.Log.Write("Allocated EAT_NameHeap with size " + sizeOfNameHeap.ToString(), Logger.ELogType.Info);
            WinAPI.ReadProcessMemory(procHandle, (IntPtr)(baseAddress + (int)lowestNameRVA), EAT_NameHeap, (IntPtr)sizeOfNameHeap, out numBytesReadAsIntPtr);
            Logger.Log.Write("Read NameHeap (bytesRead: " + numBytesReadAsIntPtr.ToString() + ")", Logger.ELogType.Info);

            // 5. process name heap into usable string collection
            int startOfCurrentStringIndex = 0;
            int numConvertedStrings = 0;
            for (int index = 0; index < EAT_NameHeap.Length; index++)
            {
                if (EAT_NameHeap[index] == 0)
                {
                    string curString = Encoding.ASCII.GetString(EAT_NameHeap, startOfCurrentStringIndex, index - startOfCurrentStringIndex);
                    for (int nameRVAIndex = 0; nameRVAIndex < EAT_NameRVAs.Length; nameRVAIndex++)
                    {
                        uint curNameRVA = EAT_NameRVAs[nameRVAIndex];
                        curNameRVA = curNameRVA - lowestNameRVA;
                        if (curNameRVA == startOfCurrentStringIndex)
                        {
                            EAT_ProcessedNames.Add((uint)nameRVAIndex, curString);
                            numConvertedStrings += 1;
                        }
                    }
                    startOfCurrentStringIndex = index + 1;
                }
            }
            Logger.Log.Write("Processed " + numConvertedStrings.ToString() + " strings from NameHeap into convenient dictionary", Logger.ELogType.Info);

            hasEAT = true;
            return true;
        }

        public static PEHeader ParseFromProcess(Process proc)
        {
            PEHeader parsedHeader = new PEHeader();
            IntPtr procHandle = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, 0, (uint)proc.Id);
            IntPtr procBaseAddr = ProcessUtils.GetProcessBase(procHandle);
            Logger.Log.Write("Got process base address: " + procBaseAddr.ToString("X2"), Logger.ELogType.Info);

            if (ParseFromAddress(procHandle, procBaseAddr, ref parsedHeader) == false)
            {
                Logger.Log.WriteError("Could not parse PE header at " + procBaseAddr.ToString("X2"));
                parsedHeader.IsValid = false;
                WinAPI.CloseHandle(procHandle);
                return parsedHeader;
            }
            parsedHeader.IsValid = true;
            WinAPI.CloseHandle(procHandle);
            return parsedHeader;
        }

        public static PEHeader ParseModuleHeader(Process proc, string moduleNameIncludingExt)
        {
            PEHeader parsedHeader = new PEHeader();

            IntPtr procHandle = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, 0, (uint)proc.Id);
            IntPtr moduleBaseAddr = ProcessUtils.GetModuleBase(procHandle, moduleNameIncludingExt);

            if (ParseFromAddress(procHandle, moduleBaseAddr, ref parsedHeader) == false)
            {
                Logger.Log.WriteError("Could not parse PE header at " + moduleBaseAddr.ToString("X2"));
                parsedHeader.IsValid = false;
                WinAPI.CloseHandle(procHandle);
                return parsedHeader;
            }
            parsedHeader.IsValid = true;
            WinAPI.CloseHandle(procHandle);
            return parsedHeader;
        }

        static bool ParseFromAddress(IntPtr procHandle, IntPtr address, ref PEHeader parsedHeader)
        {
            parsedHeader.baseAddress = address;
            // 1. read the DOS header
            int numBytesRead = 0;
            parsedHeader.dosHeader = WinAPI.Rpm<IMAGE_DOS_HEADER>(procHandle, address, out numBytesRead);
            /*
            bool rpmResult = WinAPI.ReadProcessMemory(procHandle, procBaseAddr, parsedHeader.dosHeader, Marshal.SizeOf(typeof(IMAGE_DOS_HEADER)), ref numBytesRead);
            Console.WriteLine("ReadProcessMemory result: " + rpmResult.ToString());
            Console.WriteLine("Read " + numBytesRead.ToString() + " bytes");
            */
            Logger.Log.Write("DosHeader->e_lfanew: " + parsedHeader.dosHeader.e_lfanew.ToString() + "\nbytesRead: " + numBytesRead.ToString(), Logger.ELogType.Info);
            if (!parsedHeader.dosHeader.isValid)
            {
                Logger.Log.WriteError("DosHeader is not valid. Cannot continue.");
                return false;
            }
            // 2. read just the magic field from the NTHeader.OptionalHeader.Magic, this is what determines if it's x86 or x64, which we'll need to know before reading the whole NT header struct
            MagicType NTHeaderMagic = (MagicType)WinAPI.Rpm<ushort>(procHandle, address + parsedHeader.dosHeader.e_lfanew + 24, out numBytesRead);
            Logger.Log.Write("NTHeader->OptionalHeader->Magic: " + NTHeaderMagic.ToString() + "\nbytesRead: " + numBytesRead.ToString(), Logger.ELogType.Info);
            // 3. read the appropriate NT header struct (IMAGE_NT_HEADERS32 or IMAGE_NT_HEADERS64 depending on magic field)
            if (NTHeaderMagic == MagicType.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
            {
                parsedHeader.ntHeader32 = WinAPI.Rpm<IMAGE_NT_HEADERS32>(procHandle, address + parsedHeader.dosHeader.e_lfanew, out numBytesRead);
                parsedHeader.Is64Bit = false;

                if (parsedHeader.ntHeader32.isValid == false)
                {
                    Logger.Log.WriteError("NTHeader32 is not valid. Cannot continue.");
                    return false;
                }
            }
            else if (NTHeaderMagic == MagicType.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
            {
                parsedHeader.ntHeader64 = WinAPI.Rpm<IMAGE_NT_HEADERS64>(procHandle, address + parsedHeader.dosHeader.e_lfanew, out numBytesRead);
                parsedHeader.Is64Bit = true;

                if (parsedHeader.ntHeader64.isValid == false)
                {
                    Logger.Log.WriteError("NTHeader64 is not valid. Cannot continue.");
                    return false;
                }
            }
            else
            {
                Logger.Log.WriteError("Could not read NT header magic. Cannot continue.");
                return false;
            }
            // 4. stop there and let user decide what else needs to be read
            return true;
        }
    }
}
