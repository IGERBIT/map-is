using System.Numerics;
using client.Services;
using ImGuiNET;
using MapShared.Dto;
using NativeFileDialogSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace client.Views;

public class SchemasView : ImGuiWindow
{

    private string? _fetchError;
    private string[] _fetchAnim = new[]
    {
        "Fetching",
        "Fetching.",
        "Fetching..",
        "Fetching...",
    };

    private List<SchemaLiteDto> _schemas = new List<SchemaLiteDto>();
    
    public override void Render()
    {
        if(!IsOpen) return;
        if (!ImGui.Begin("Schemas",  ref IsOpen)) return;

        if (ImGui.Button("Update")) Fetch();
        if (_fetchError is not null) ImGui.TextUnformatted(_fetchError);

        var buttonWidth = ImGui.GetContentRegionAvail().X;
        
        for (var i = 0; i < _schemas.Count; i++)
        {
            var schema = _schemas[i];
            
            if (ImGui.Button(schema.Name + "##" + i, new Vector2(buttonWidth, 30)))
            {
                OnOpenScheme(schema.Id);
            }
            
            if (ImGui.BeginPopupContextItem())
            {
                ImGui.Text("Id: " + schema.Id);
                if (ImGui.Button("Edit")) OnSchemeEdit(schema.Id);
                if (ImGui.Button("Delete")) ImGui.OpenPopup("delete");
                if (ImGui.BeginPopup("delete"))
                {
                    if (ImGui.Button("Confirm")) OnSchemeDelete(schema.Id); 
                    ImGui.EndPopup();
                }
                ImGui.EndPopup();
            }
            
        }
        
        ImGui.PushStyleColor(ImGuiCol.Button, F.Color(50,170,50));
        
        if (ImGui.Button("Create New##create", new Vector2(buttonWidth, 30)))
            ImGui.OpenPopup("Create Schema");
        ImGui.PopStyleColor();
        
        CreateSchemeModal();
        
        ImGui.End();
    }

    private void OnSchemeDelete(int schemaId)
    {
        throw new NotImplementedException();
    }

    private void OnSchemeEdit(int schemaId)
    {
        Windows.Get<MapEditorView>().Setup(schemaId, Image.Load("map.jpg"), new SchemaDto());
        Windows.Open<MapEditorView>();
    }

    private bool _createSchemeModal;
    private string _createSchemeName = "";
    private string _createSchemePath = "";
    private string _createError = "";

    private void CreateSchemeModal()
    {
        if(!ImGuiF.BeginPopupModal("Create Schema", ref _createSchemeModal)) return;
        
        ImGui.TextUnformatted("Name");
        ImGui.InputText("##scheme_name", ref _createSchemeName, 32);
        ImGui.TextUnformatted("Image");
        ImGui.InputText("##scheme_image_path", ref _createSchemePath, 128);
        ImGui.SameLine();
        if (ImGui.Button("Browse"))
        {
            var fileOpenResult = Dialog.FileOpen("png,jpg,bmp");
            if (fileOpenResult.IsOk)
            {
                _createSchemePath = fileOpenResult.Path;
            }
        }
        
        ImGui.TextUnformatted(_createError);
        if (ImGui.Button("Create")) OnCreateScheme();
        ImGui.EndPopup();
    }
    
    
    
    private async void OnCreateScheme()
    {
        using var _ = State.BeginDisableScope();
        if (string.IsNullOrEmpty(_createSchemePath))
        {
            _createError = "Path is incorrect";
            return;
        }
        try
        {
                
            var image = await Image.LoadAsync(_createSchemePath);
            var pngStream = new MemoryStream();
            await image.SaveAsPngAsync(pngStream);

            pngStream.Seek(0, SeekOrigin.Begin);

            var result = await Service<NetService>().CreateSchema(_createSchemeName, pngStream).ValueOrThrow();
                
            ImGui.CloseCurrentPopup();
                
        }
        catch (Exception e)
        {
            _createError = e.Message;
            return;
        }
    }

    private void OnOpenScheme(int schemaId)
    {
        
    }

    private async void Fetch()
    {
        using var _ = State.BeginDisableScope();
        try
        {
            _fetchError = null;
            var schemas = await Service<NetService>().GetSchemes().ValueOrThrow();
            
            _schemas.Clear();
            _schemas.AddRange(schemas);
        }
        catch (Exception e)
        {
            _fetchError = e.Message;
        }
    }
}

