using System.Drawing;
using System.Reflection;
using ClickableTransparentOverlay;
using client.Views;

namespace client;

public class Program : Overlay
{
    private readonly WindowsManager _windowsManager;

    protected override void Render()
    {
        _windowsManager.Render();
    }

    public Program(string windowTitle, bool DPIAware) : base(windowTitle, DPIAware)
    {
        _windowsManager = new WindowsManager(this);
    }

    
    protected override Task PostInitialized()
    {
        VSync = true;
        Size = new Size(User32.GetSystemMetrics(User32.SM_CXSCREEN), User32.GetSystemMetrics(User32.SM_CYSCREEN));
        _windowsManager.Open<LoginWindow>();

        var fieldFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var windowField = typeof(Overlay).GetField("window",fieldFlags);
        var win32WindowType = windowField!.FieldType;
        var handleField = win32WindowType.GetField("Handle", fieldFlags);
        
        
        var win32Window = windowField.GetValue(this);
        var handle = (nint)handleField!.GetValue(win32Window)!;

        User32.SetWindowPos(handle, -2, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0040);

        return Task.CompletedTask;
    }

    public static async Task Main(string[] args)
    {
        using var p = new Program("Map Client", true);
        await p.Run();
    }
}