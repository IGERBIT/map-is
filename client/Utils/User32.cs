using System.Runtime.InteropServices;

namespace client;

internal static class User32
{
    private const string LibraryName = "user32.dll";

    internal const int SM_CXSCREEN = 0;
    internal const int SM_CYSCREEN  = 1;
    
    [DllImport(LibraryName)]
    public static extern int GetSystemMetrics ([In] int index);
    
    [DllImport(LibraryName)]
    public static extern int SetWindowPos([In] IntPtr hWnd, [In, Optional]IntPtr hWndInsertAfter, int x, int y, int cX, int cY, uint flags);
}
