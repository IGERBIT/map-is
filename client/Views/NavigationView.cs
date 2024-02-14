using ImGuiNET;

namespace client.Views;

public class NavigationView : ImGuiWindow
{

    public string[] list =
        "Use if you want to re-implement ListBox() will custom data or interactions. if the function return true, you can output elements then call ListBoxFooter() afterwards.\n"
            .Split(" ")
            .Select(x => x.Trim())
            .Where(x=> !string.IsNullOrEmpty(x))
            .ToArray();


    private readonly ListBoxWithSearch _search1;

    public NavigationView()
    {
        _search1 = new ListBoxWithSearch("##21", list, 65, 10);
    }

    private int current = -1;
    private int end = -1;

    public override void Render()
    {
        if (!IsOpen || !ImGui.Begin("Navigation", ref IsOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse)) return;

        if (_search1.Render())
        {
            if (current == _search1.CurrentItem)
            {
                end = _search1.CurrentItem;
                ImGui.OpenPopup("currentShow");
            }
            current = _search1.CurrentItem;
        }
        
        if(_search1.CurrentItem >= 0) ImGui.Text($"Selected: {list[_search1.CurrentItem]} ");
        if(current >= 0) ImGui.Text($"current: {list[current]} ");

        if (ImGuiF.BeginPopupModal("currentShow", ref _modal))
        {
            ImGui.Text($"Result: {list[end]} ");
            
            if(ImGui.Button("X")) ImGui.CloseCurrentPopup();
            
            ImGui.EndPopup();
        }
        
        ImGui.End();

       
    }


    private bool _modal;





}