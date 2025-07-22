using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CursorCloak;

internal class ShellChecks
{
    public static bool StartedByDoubleClick()
    {
        int ppid = GetParentProcessId(Process.GetCurrentProcess().Handle);
        try
        {
            var parent = Process.GetProcessById(ppid);
            string name = parent.ProcessName.ToLowerInvariant();
            return name == "explorer";
        }
        catch
        {
            return false;
        }
    }

    [DllImport("ntdll.dll")]
    private static extern int NtQueryInformationProcess(
        IntPtr processHandle, int processInformationClass,
        ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);

    [StructLayout(LayoutKind.Sequential)]
    struct PROCESS_BASIC_INFORMATION
    {
        public IntPtr Reserved1;
        public IntPtr PebBaseAddress;
        public IntPtr Reserved2_0;
        public IntPtr Reserved2_1;
        public IntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
    }

    static int GetParentProcessId(IntPtr handle)
    {
        var pbi = new PROCESS_BASIC_INFORMATION();
        NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out _);
        return pbi.InheritedFromUniqueProcessId.ToInt32();
    }
}
