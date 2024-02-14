using System.Numerics;
using ImGuiNET;

namespace client.Views;

public class ExitView : ImGuiWindow
{
    private bool exitConfirmation = false;
    
    public override void Render()
    {
        if (Windows.Windows.All(x => !x.IsOpen) && !exitConfirmation) IsOpen = true;
        
        var displaySize = ImGui.GetIO().DisplaySize;
        if(!IsOpen) return;
        ImGui.SetNextWindowPos(new Vector2(displaySize.X / 2, displaySize.Y / 2), ImGuiCond.None, new Vector2(0.5f, 0.5f));
        if(!ImGui.Begin("Exit##Exit", ref IsOpen, (ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize) & ~ImGuiWindowFlags.NoTitleBar)) return;

        if (ImGui.Button("Exit"))
        {
            exitConfirmation = true;
            IsOpen = false;
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Back"))
        {
            Windows.Open<LoginWindow>();
            IsOpen = false;
        }
        
        
        ImGui.End();
    }
}

