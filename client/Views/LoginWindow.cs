using System.Numerics;
using client.Services;
using client.Utils;
using ImGuiNET;
using MapShared.Dto;

namespace client.Views;

public class LoginWindow : ImGuiWindow
{

    private bool _createOrg;
    
    private string _singInEmail = "";
    private string _singInPassword = "";
    
    
    private string _orgName = "";
    private string _orgArea = "";
    
    private string _orgPhone = "";
    private string _orgAddress = "";
    private string _orgSite = "";
    
    private string _ownerPassword = "";
    private string _ownerFullname = "";
    private string _ownerEmail = "";

    private ImFontPtr _passwordFont;
    
    private unsafe void InitFonts()
    {
        Overlay.ReplaceFont(config =>
        {
            var io = ImGui.GetIO();
            var fonts = io.Fonts;
            fonts.AddFontDefault(config);
            fixed (ushort* ranges = new ushort[] { 0x002a, 0x002b, 0 })
            {
                _passwordFont = fonts.AddFontFromFileTTF("fonts/droid_sans.ttf", 16f, config, new nint(ranges));
                _passwordFont.FallbackChar = '*';
            }
        });
    }

    private const float OffsetXSingIn = 80;
    private const float OffsetXSingUp = 120;

    private string _signInError;
    
    public override void Render()
    {
        var displaySize = ImGui.GetIO().DisplaySize;
        if(!IsOpen) return;
        ImGui.SetNextWindowPos(new Vector2(displaySize.X / 2, displaySize.Y / 2), ImGuiCond.None, new Vector2(0.5f, 0.5f));
        if(!ImGui.Begin("Login##Login", ref IsOpen, (ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize) & ~ImGuiWindowFlags.NoTitleBar)) return;

        if(_createOrg) CreateOrganization();
        else SignInView();
        
        
        ImGui.End();
    }

    private void SignInView()
    {
        ImGui.TextUnformatted("Email"); ImGui.SameLine(OffsetXSingIn);
        ImGui.InputText("##1", ref _singInEmail, 64);
        
        ImGui.TextUnformatted("Password"); ImGui.SameLine(OffsetXSingIn);
        ImGui.InputText("##2", ref _singInPassword, 64, ImGuiInputTextFlags.Password);

        ImGui.NewLine();
        
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, F.Color(255,0,0));
        ImGui.PushTextWrapPos();
        ImGui.TextUnformatted(_signInError);
        ImGui.PopTextWrapPos();
        ImGui.PopStyleColor();
        
        
        ImGui.NewLine();
        
        
        
        
        var buttonWidth = ImGui.GetContentRegionAvail().X;
        if (ImGui.Button("Sing In", new Vector2(buttonWidth, 30))) OnSingInClicked();
        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Button("Skip")) Skip();
            if (ImGui.Button("Skip As User")) SkipUser();
            if (ImGui.Button("Skip With Delay")) SkipDelay();
            ImGui.EndPopup();
        }
        if (ImGui.Button("Create Organization", new Vector2(buttonWidth, 30))) _createOrg = true;
    }

    private void Skip()
    {
        ImGui.CloseCurrentPopup();
        Windows.Open<OrganizationControlView>();
        State.IsOwner = true;
        State.Debug = true;
        IsOpen = false;
    }
    
    private void SkipUser()
    {
        ImGui.CloseCurrentPopup();
        Windows.Open<OrganizationControlView>();
        State.IsOwner = false;
        State.Debug = true;
        IsOpen = false;
    }
    
    private async void SkipDelay()
    {
        ImGui.CloseCurrentPopup();
        using var _ = State.BeginDisableScope();

        await Task.Delay(TimeSpan.FromSeconds(3));
        
        Windows.Open<OrganizationControlView>();
        State.IsOwner = true;
        State.Debug = true;
        IsOpen = false;
    }

    private void CreateOrganization()
    {
        
        ImGuiF.TextCentered("Organization Information");
        ImGui.TextUnformatted("Name"); ImGui.SameLine(OffsetXSingIn); 
        ImGui.InputText("##org_name", ref _orgName, 64);

        ImGui.TextUnformatted("Area"); ImGui.SameLine(OffsetXSingIn); 
        ImGui.InputText("##org_area", ref _orgArea, 64);
        ImGui.NewLine();
        
        ImGuiF.TextCentered("Contact information");
        ImGui.TextUnformatted("Site"); ImGui.SameLine(OffsetXSingIn); 
        ImGui.InputText("##org_site", ref _orgSite, 64);
        
        ImGui.TextUnformatted("Phone"); ImGui.SameLine(OffsetXSingIn); 
        ImGui.InputText("##org_phone", ref _orgPhone, 64);
        
        ImGui.TextUnformatted("Address"); ImGui.SameLine(OffsetXSingIn); 
        ImGui.InputText("##org_addr", ref _orgAddress, 64);
        ImGui.NewLine();
        
        ImGuiF.TextCentered("Owner info:");
        
        ImGui.TextUnformatted("Fullname:"); ImGui.SameLine(OffsetXSingIn);
        ImGui.InputText("##fullname", ref _ownerFullname, 64);
        
        ImGui.TextUnformatted("Email"); ImGui.SameLine(OffsetXSingIn); 
        ImGui.InputText("##1", ref _ownerEmail, 64);
                
        ImGui.TextUnformatted("Password"); ImGui.SameLine(OffsetXSingIn);
        ImGui.InputText("##2", ref _ownerPassword, 64, ImGuiInputTextFlags.Password);
        PasswordChecks();
        
        
        ImGui.NewLine();
        
        
        
        var buttonWidth = ImGui.GetContentRegionAvail().X;
        if (ImGui.Button("Sing Up", new Vector2(buttonWidth, 30))) OnSingUpClicked();
        if (ImGui.Button("I have an account", new Vector2(buttonWidth, 30))) _createOrg = false;
    }

    private void PasswordChecks()
    {
        ImGui.NewLine();
        
        if(string.IsNullOrEmpty(_ownerPassword)) return;

        string? error = null;
        
        if (_ownerPassword.Length < 8)
            error = "It must be at least 8 chars length.";
        
        else if (!_ownerPassword.Any(char.IsUpper))
            error = "It must have at least one capital letter";
        
        else if (!_ownerPassword.Any(char.IsDigit))
            error = "It must have at least one digit";

        if (error is null) return;
        
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, F.Color(255,0,0));
        ImGui.PushTextWrapPos();
        ImGui.TextUnformatted(error);
        ImGui.PopTextWrapPos();
        ImGui.PopStyleColor();

    }

    private void OnExitClicked()
    {
        IsOpen = false;
    }

    private async void OnSingInClicked()
    {
        using var _ = State.BeginDisableScope();
        try
        {
            var result = await Service<NetService>().SingIn(new SignInDto(_singInEmail, _singInPassword));
            var token = result.ValueOrThrow();
            
            Service<NetService>().SetToken(token.Token);
            
            State.Token = token.Token;
            State.IsOwner = token.IsOwner;
            
            Windows.Open<OrganizationControlView>();
            IsOpen = false;
        }
        catch (ApiException e)
        {
            _signInError = $"{e.Message}";
        }
    }
    
    private async void OnSingUpClicked()
    {
        using var _ = State.BeginDisableScope();
        try
        {
            var result = await Service<NetService>().CreateOrg(new CreateOrganizationDto(
                _orgName, 
                _orgArea, 
                new ContactsDto(_orgSite, _orgPhone, _orgAddress),
                new OwnerInfoDto(_ownerFullname, _ownerEmail, _ownerPassword)));
            
            result.ValueOrThrow();

            _singInEmail = _ownerEmail;
            _singInPassword = "";

            _createOrg = false;
        }
        catch (ApiException e)
        {
            HandleException(e);
        }
    }

   

    
     

    

    

    
}

