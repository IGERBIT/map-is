namespace client;

public static class State
{
    public static string? Token { get; set; }
    public static bool IsOwner { get; set; }
    public static bool Debug { get; set; }
    public static int DisableAmount { get; set; }


    public static DisableContext BeginDisableScope() => new DisableContext();
    
    public class DisableContext : IDisposable
    {
        private bool _disposed;
        
        public DisableContext()
        {
            DisableAmount++;
        }

        public void Dispose()
        {
            if(_disposed) return;
            DisableAmount--;
            _disposed = true;
        }
    }
}

