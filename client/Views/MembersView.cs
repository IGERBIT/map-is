using System.Numerics;
using client.Services;
using ImGuiNET;
using MapShared.Dto;

namespace client.Views;

public class MembersView : ImGuiWindow
{

    private List<MemberDto> _members = new List<MemberDto>();
    
    public override void Render()
    {
        if(!IsOpen) return;
        if (!ImGui.Begin("Members",  ref IsOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize)) return;

        if (ImGui.Button("Reload")) LoadMembers();
        
        if(_fetchError is not null) ImGui.TextUnformatted(_fetchError);
        
        bool openEdit = false;
        
        if (ImGui.BeginTable("table2", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("FullName", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Email", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Assign Date", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 100);
            
            
            ImGui.TableHeadersRow();
            for (var i = 0; i < _members.Count; i++)
            {
                ImGui.TableNextRow();
                ImGui.PushID(i);
                var member = _members[i];
            
                ImGui.TableNextColumn(); ImGui.TextUnformatted(member.FullName);
                ImGui.TableNextColumn(); ImGui.TextUnformatted(member.Email);
                ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.AssignDate:G}");
                ImGui.TableNextColumn(); 
                if (ImGui.Button("Edit")) OnMemberEdit(member.Id);
                ImGui.SameLine();
                if (ImGui.Button("Revoke")) ImGui.OpenPopup("member_delete"); 
                if (ImGui.BeginPopupContextItem("member_delete"))
                {
                    if (ImGui.Button("Remove Member"))
                    {
                        OnMemberDelete(member.Id);
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
                ImGui.PopID();
            
            }
            ImGui.EndTable();
        }

        if (_openEdit)
        {
            ImGui.OpenPopup("Member Edit"); 
            _openEdit = false;
        }
        
        if (ImGui.Button("Add Member")) OnCreate(); 
        
        
        EditPopup();
        CreatePopup();
        
        ImGui.End();
    }

    public async void LoadMembers()
    {
        using var _ = State.BeginDisableScope();
        try
        {
            _fetchError = null;
            var members = await Service<NetService>().Members().ValueOrThrow();
            
            _members.Clear();
            _members.AddRange(members);
        }
        catch (Exception e)
        {
            _fetchError = e.Message;
        }
    }

    private void EditPopup()
    {
        if (ImGuiF.BeginPopupModal("Member Edit", ref _editVisible, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize))
        {
            ImGui.TextUnformatted("Fullname:");
            ImGui.InputText("##edit_name", ref _editFullName, 32);
            ImGui.TextUnformatted("Email:");
            ImGui.InputText("##edit_email", ref _editEmail, 32);
            ImGui.Checkbox("##change_pass", ref _changePassword);
            ImGui.SameLine();
            ImGui.TextUnformatted("New Password:");
            ImGui.BeginDisabled(!_changePassword);
            ImGui.InputText("##edit_pass", ref _editNewPassword, 32, ImGuiInputTextFlags.Password);
            ImGui.EndDisabled();

            if (ImGui.Button("Save")) OnEditSave(); ImGui.SameLine();
            if (ImGui.Button("Close")) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }
    
    private void CreatePopup()
    {
        if (ImGuiF.BeginPopupModal("New Member", ref _createVisible, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize))
        {
            ImGui.TextUnformatted("Fullname:");
            ImGui.InputText("##create_name", ref _createFullName, 32);
            ImGui.TextUnformatted("Email:");
            ImGui.InputText("##create_email", ref _createEmail, 32);
            ImGui.TextUnformatted("Password:");
            ImGui.InputText("##create_pass", ref _createNewPassword, 32, ImGuiInputTextFlags.Password);

            if (ImGui.Button("Add")) OnCreateSave(); ImGui.SameLine();
            if (ImGui.Button("Back")) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    private void OnCreate()
    {
        ImGui.OpenPopup("New Member");
    }

    private async void OnEditSave()
    {
        using var _ = State.BeginDisableScope();
        if(_editMemberId is null) return;
        try
        {
            _fetchError = null;
            await Service<NetService>().UpdateMember(_editMemberId.Value, new CreateMemberDto
            {
                FullName = _editFullName,
                Email = _editEmail,
                Password = _changePassword ? _editNewPassword : null
            }).ValueOrThrow();
            
            _editFullName = "";
            _editEmail = "";
            _editNewPassword = "";
            _changePassword = false;

            _editVisible = false;
            
            LoadMembers();
        }
        catch (Exception e)
        {
            _fetchError = e.Message;
        }
    }
    
    private async void OnCreateSave()
    {
        using var _ = State.BeginDisableScope();
        try
        {
            _fetchError = null;
            await Service<NetService>().AddMember(new CreateMemberDto
            {
                FullName = _createFullName,
                Email = _createEmail,
                Password = _createNewPassword
            }).ValueOrThrow();

            _createFullName = "";
            _createEmail = "";
            _createNewPassword = "";

            _createVisible = false;
            
            LoadMembers();
            
            
        }
        catch (Exception e)
        {
            _fetchError = e.Message;
        }
    }

    private bool _openEdit;
    private bool _editVisible;
    private bool _createVisible;
    
    private Guid? _editMemberId;
    private string _editFullName = "";
    private string _editEmail = "";
    private string _editNewPassword = "";
    private bool _changePassword;
    
    private string _createFullName = "";
    private string _createEmail = "";
    private string _createNewPassword = "";
    private string? _fetchError;

    private async void OnMemberDelete(Guid memberId)
    {
        using var _ = State.BeginDisableScope();
        try
        {
            _fetchError = null;
            await Service<NetService>().RemoveMember(memberId).ValueOrThrow();
            
            LoadMembers();
            
        }
        catch (Exception e)
        {
            _fetchError = e.Message;
        }
        
    }

    private void OnMemberEdit(Guid memberId)
    {
        var member = _members.FirstOrDefault(x => x.Id == memberId);
        if(member is null) return;
        
        _editMemberId = memberId;
        _editFullName = member.FullName;
        _editEmail = member.Email;
        _editNewPassword = "";
        _changePassword = false;

        _openEdit = true;
    }
}

