using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;

namespace client;

public static unsafe class F
{
    public static uint Color(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }
    public static uint Color(byte r, byte g, byte b) => Color(r, g, b, 255);

    public static uint Color(float r, float g, float b, float a) => Color((byte) (r * 255), (byte) (g * 255), (byte) (b * 255), (byte) (a * 255));
    public static uint Color(float r, float g, float b) => Color(r, g, b, 1f);
    
    public static int GetUtf8(ReadOnlySpan<char> s, byte* utf8Bytes, int utf8ByteCount)
    {
      if (s.IsEmpty)
        return 0;
      fixed (char* chars = &s.GetPinnableReference())
        return Encoding.UTF8.GetBytes(chars, s.Length, utf8Bytes, utf8ByteCount);
    }

    public static byte* AllocUtf8String(ReadOnlySpan<char> chars)
    {
        if (chars == null) return null;
        var utf8ByteCount = Encoding.UTF8.GetByteCount(chars);
        var pointer = (byte*)Marshal.AllocHGlobal(utf8ByteCount + 1).ToPointer();
        var actualLength = GetUtf8(chars, pointer, utf8ByteCount);
        pointer[actualLength] = 0;
        return pointer;
    }
    
    
    public static void FreeUtf8String(byte* str)
    {
        Marshal.FreeHGlobal(new IntPtr(str));
    }

   
}

