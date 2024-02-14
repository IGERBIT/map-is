using ClickableTransparentOverlay;

namespace client;

public abstract class ImGuiWindow
{
    public bool IsOpen = false;
    

    public WindowsManager Windows { get; set; }

    public Overlay Overlay => Windows.Overlay;
    public HttpClient HttpClient => Windows.HttpClient;
    public Config Config => Windows.Config;

    public T Service<T>() => Windows.Service<T>();

    public virtual void Init() {}
    
    public abstract void Render();
}

