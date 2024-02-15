using ImGuiNET;

namespace client.Views;

public class MembersView : ImGuiWindow
{
    public override void Render()
    {
        if(!IsOpen) return;
        if (!ImGui.Begin("Members",  ref IsOpen)) return;
        
        
        ImGui.End();
    }
}

