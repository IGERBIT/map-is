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

        if (ImGui.Button("Schemes", new Vector2(buttonWidth, 30)))
        {
            Windows.Open<SchemasView>();
            Windows.Get<SchemasView>().Fetch();
        }
        if (State.IsOwner)
        {
            
            if (ImGui.Button("Members", new Vector2(buttonWidth, 30)))
            {
                Windows.Open<MembersView>();
                Windows.Get<MembersView>().LoadMembers();
            }
        }
        
        
        ImGui.End();
    }


}

