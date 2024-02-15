using ImGuiNET;

namespace client.Views;

public class SchemasView : ImGuiWindow
{
    public override void Render()
    {
        if(!IsOpen) return;
        if (!ImGui.Begin("Schemas",  ref IsOpen)) return;
        
        
        ImGui.End();
    }
}

