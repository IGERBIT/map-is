﻿using System.Numerics;
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
        if (State.IsOwner)
        {
            if (ImGui.Button("Schemas", new Vector2(buttonWidth, 30))) Windows.Open<SchemasView>();
            if (ImGui.Button("Members", new Vector2(buttonWidth, 30))) Windows.Open<MembersView>();
        }
        
        
        ImGui.End();
    }


}

