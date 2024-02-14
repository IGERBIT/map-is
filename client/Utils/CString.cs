namespace client.Utils;

public unsafe class CString : IDisposable
{
    public string? Text { get; private set; }
    public byte* Ptr { get; private set; }

    public nint Pointer() => new IntPtr(Ptr);

    public CString(string? text)
    {
        SetText(text);
    }

    public void SetText(string? text)
    {
        if(text == Text) return;

        Text = text;
        
        ReleaseCString();
        Ptr = F.AllocUtf8String(text);
    }

    private void ReleaseCString()
    {
        F.FreeUtf8String(Ptr);
    }
    
    
    private void ReleaseUnmanagedResources()
    {
        ReleaseCString();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~CString()
    {
        ReleaseUnmanagedResources();
    }
}

