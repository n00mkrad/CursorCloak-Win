using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CursorCloak;
internal class WinCursor
{
    static WinCursor()
    {
        Reload(); // Ensure the system cursors are loaded
    }

    private static bool _visible = true;

    // All standard system cursor IDs
    private static readonly uint[] _ids =
    {
            32512, 32513, 32514, 32515, 32516,
            32642, 32643, 32644, 32645, 32646,
            32648, 32649, 32650
        };

    private class Handles
    {
        public IntPtr Default;
        public IntPtr Blank;
    }

    private static readonly Dictionary<uint, Handles> _cache = new();

    /// <summary>Show all system cursors.</summary>
    public static void Show()
    {
        _visible = true;
        Apply();
        Reload(true);
    }

    /// <summary>Hide all system cursors.</summary>
    public static void Hide()
    {
        _visible = false;
        Apply();
    }

    /// <summary>
    /// Reload internal cache. If restoreWindowsDefaults is true, calls SPI_SETCURSORS.
    /// </summary>
    public static void Reload(bool restoreWindowsDefaults = false)
    {
        if (restoreWindowsDefaults)
        {
            // Restores original cursors from the system resources
            SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, 0);
            _cache.Clear();
            _visible = true;
            return;
        }

        if (_cache.Count > 0) return;

        foreach (var id in _ids)
        {
            IntPtr hCur = LoadCursor(IntPtr.Zero, (IntPtr)id);
            CheckWin32(hCur != IntPtr.Zero, "LoadCursor");

            IntPtr hDefault = CopyImage(hCur, IMAGE_CURSOR, 0, 0, 0);
            CheckWin32(hDefault != IntPtr.Zero, "CopyImage default");

            // Create a 32x32 blank cursor (all AND mask = 0xFF, XOR mask = 0x00)
            const int size = 32 * 4; // 32x32 bits => 32*32/8 = 128 bytes; DWORD alignment – using 32*4 for simplicity as in AHK code
            var andMask = new byte[size];
            var xorMask = new byte[size];
            for (int i = 0; i < andMask.Length; i++) andMask[i] = 0xFF;

            IntPtr pAnd = Marshal.AllocHGlobal(size);
            IntPtr pXor = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(andMask, 0, pAnd, size);
                Marshal.Copy(xorMask, 0, pXor, size);

                IntPtr hBlank = CreateCursor(IntPtr.Zero, 0, 0, 32, 32, pAnd, pXor);
                CheckWin32(hBlank != IntPtr.Zero, "CreateCursor");

                _cache[id] = new Handles { Default = hDefault, Blank = hBlank };
            }
            finally
            {
                Marshal.FreeHGlobal(pAnd);
                Marshal.FreeHGlobal(pXor);
            }
        }
    }

    private static void Apply()
    {
        if (_cache.Count == 0) Reload();

        foreach (var kvp in _cache)
        {
            var src = _visible ? kvp.Value.Default : kvp.Value.Blank;
            IntPtr hCopy = CopyImage(src, IMAGE_CURSOR, 0, 0, 0);
            CheckWin32(hCopy != IntPtr.Zero, "CopyImage apply");

            // Replaces the system cursor for this id
            CheckWin32(SetSystemCursor(hCopy, kvp.Key), "SetSystemCursor");
        }
    }

    #region Win32

    private const uint SPI_SETCURSORS = 0x0057;
    private const uint IMAGE_CURSOR = 2;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CopyImage(IntPtr hImage, uint uType, int cxDesired, int cyDesired, uint fuFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetSystemCursor(IntPtr hcur, uint id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateCursor(
        IntPtr hInst,
        int xHotSpot,
        int yHotSpot,
        int nWidth,
        int nHeight,
        IntPtr pvANDPlane,
        IntPtr pvXORPlane);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    private static void CheckWin32(bool ok, string api)
    {
        if (!ok)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"{api} failed");
        }
    }

    #endregion
}

