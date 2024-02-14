using System.Numerics;
using ImGuiNET;

namespace client.Views;

public class OrganizationControlView : ImGuiWindow
{

    private string _whoiam;
    public override void Render()
    {
        if (!IsOpen || !ImGui.Begin("Org", ref IsOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse)) return;
        
        var buttonWidth = ImGui.GetContentRegionAvail().X;

        if (ImGui.Button("Navigate", new Vector2(buttonWidth, 30))) Windows.Open<NavigationView>();
        ImGui.Button("Maps Management", new Vector2(buttonWidth, 30));
        ImGui.Button("Members Management", new Vector2(buttonWidth, 30));

        if (ImGui.Button("Who I Am", new Vector2(buttonWidth, 30))) OnWhoIAm();
        
        ImGui.TextUnformatted($"Who: ${_whoiam}");
        
        ImGui.End();
    }

    private async void OnWhoIAm()
    {
        var result = await Service<NetService>().WhoIAm();

        if (result.IsFail) _whoiam = $"Error: {result.Error}";
        else _whoiam = result.Value!;
    }
}

