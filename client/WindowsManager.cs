using System.Reflection;
using ClickableTransparentOverlay;
using client.Services;
using ImGuiNET;

namespace client;

public class WindowsManager
{
    public readonly ImGuiWindow[] Windows;
    
    public Overlay Overlay { get; }
    public HttpClient HttpClient { get; }
    public Config Config { get; }
    private readonly object[] _services;
    
    private bool _firstFrameInit;
    private SynchronizationContext? _original;
    private readonly DelayedSynchronizationContext _current = new DelayedSynchronizationContext();

    private readonly List<Exception> _exceptions = new();
    

    public WindowsManager(Overlay overlay)
    {
        Overlay = overlay;
        HttpClient = new HttpClient();
        Config = new Config("settings.json");
        Windows = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(ImGuiWindow)) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .OfType<ImGuiWindow>()
            .ToArray();

        _services = new[]
        {
            new NetService()
        };
        
        

        for (var i = 0; i < Windows.Length; i++)
        {
            Windows[i].Windows = this;
        }
    }

    public T Service<T>()
    {
        return _services.OfType<T>().FirstOrDefault()!;
    }

    public void Render()
    {
        if (!_firstFrameInit)
        {
            _original = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(_current);
            _firstFrameInit = true;
            
            for (var i = 0; i < Windows.Length; i++)
                Windows[i].Init();
        }
        
        bool atLeastOneOpen = false;
        
        try
        {
           
            for (var i = 0; i < Windows.Length; i++)
            {
                var window = Windows[i];

                if (window.IsOpen) atLeastOneOpen = true;
                ImGui.BeginDisabled(State.DisableAmount > 0);
                window.Render();
                ImGui.EndDisabled();
                if (window.IsOpen) atLeastOneOpen = true;
            }

            _current.ProcessMessages();
        }
        catch (Exception e)
        {
            HandleException(e);
        }

        if (!atLeastOneOpen)
        {
            Overlay.Close();
        }


        RenderExceptions();
    }

    private int _exceptionIndex = 0;

    public void HandleException(Exception e)
    {
        _exceptions.Add(e);
    }

    private void RenderExceptions()
    {
        var count = _exceptions.Count;
        if(count < 1) return;

        if (_exceptionIndex >= count) _exceptionIndex = count - 1;
        if (_exceptionIndex < 0) _exceptionIndex = 0;

        var e = _exceptions[_exceptionIndex];

        ImGui.Begin("Exceptions");
        

        if (count > 1)
        {
            ImGui.TextUnformatted($"{_exceptionIndex + 1} / {count}\n");
            ImGui.SameLine();
            if (ImGui.Button("<<")) _exceptionIndex--;
            ImGui.SameLine();
            if (ImGui.Button(">>")) _exceptionIndex++;
        

        
            if (_exceptionIndex >= count) _exceptionIndex = 0;
            if (_exceptionIndex < 0) _exceptionIndex = count - 1;
        }
        
        if (ImGui.Button("Close")) _exceptions.RemoveAt(_exceptionIndex);

        if (count > 1)
        {
            ImGui.SameLine();
            if (ImGui.Button("Close All")) _exceptions.Clear();
        }
        
        ImGui.TextUnformatted(e.ToString());
        ImGui.End();
    }

    public bool Open<T>() where T : ImGuiWindow
    {
        if (Get<T>() is not { } window) return false;
        window.IsOpen = true;
        return true;
    }
    
    public bool Close<T>() where T : ImGuiWindow
    {
        if (Get<T>() is not { } window) return false;
        window.IsOpen = false;
        return true;
    }
    
    public T Get<T>() where T : ImGuiWindow
    {
        return Windows.OfType<T>().FirstOrDefault()!;
    }
    
}
