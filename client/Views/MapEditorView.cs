using client.Services;
using ImGuiNET;
using MapShared.Dto;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vortice.Mathematics;
using Size = Vortice.Mathematics.Size;
using Vector2 = System.Numerics.Vector2;


namespace client.Views;

public class MapEditorView : ImGuiWindow
{
    private static readonly uint ObjectFillColor = F.Color(.1f, .1f, 1f, .5f);
    private static readonly uint ObjectFillColorHovered = F.Color(.1f, .1f, 1f, .75f);
    private static readonly uint ObjectStrokeColor = F.Color(0f, 0f, 0f);
    private static readonly float ObjectStrokeThickness = 2;
    private static readonly float ObjectNamePadding = 10f;
    
    private static readonly uint NodeFillColor = F.Color(1f, .1f, 1f, .2f);
    private static readonly uint NodeFillColorHovered = F.Color(1f, .1f, 1f, .4f);
    private static readonly float NodeImageRadius = 4f;
    private static readonly uint NodeStrokeColor = F.Color(1f, .1f, 1f, .5f);
    private static readonly float NodeStrokeThickness = 1;
    
    private static readonly uint LinkCreationColor = F.Color(.4f, 1f, .4f, .5f);
    private static readonly float LinkThickness = 2;
    private static readonly uint LinkColor = F.Color(.1f, 1f, .1f, .3f);
    private static readonly uint LinkColorHovered = F.Color(.1f, 1f, .1f, .5f);
    private static readonly uint LinkObjectColor = F.Color(.6f, 1f, .1f, .3f);
    private static readonly uint LinkObjectColorHovered = F.Color(.6f, 1f, .1f, .5f);
    
    private nint _ptr;
    private Size _imageSize;
    
    private Rect _winRect;
    private Rect _imgView;
    private Rect _imgRect;
    private Rect _sideRect;

    private Vector2 _image2screen;

    private List<NodeDto> _nodes = new ();
    private List<MapObjectDto> _objects = new();
    private List<LinkDto> _links = new();
    private List<ObjectLinkDto> _objectLinks = new();

    private int _schemeId;
    private string _schemeName = "";
    private int _nextObjectId = -1;
    private int _nextNodeId = -1;
    private int _nextLinkId = -1;

    
    private object? _contextMenuTarget;
    private object? _entityUnderMouse;
    private Vector2 _mouseScreenPosition;
    private Vector2? _mouseImagePosition;

    private const float NodeInteractRadius = 4f;
    private const float LinkInteractRadius = 3f;
    private const float ObjectCornerInteractRadius = 4f;

    private Rect? _selectedArea;
    private Rect? _currentSelectedArea;
    private string _newObjectName;

    private Vector2 _imageOffset;
    private float _zoom = 1f;
    private Vector2 _lastMiddleDelta;

    private bool _linkCreation;
    private NodeDto? _linkedNode;

    private object? _draggedEntity;
    private Vector2 _lastDragDelta;
    private ObjectDragType _dragType = ObjectDragType.MoveObject;
    private enum ObjectDragType
    {
        MoveObject,
        Corner
    }

    private bool _showObjects = true;
    private bool _showNodes = true;
    private bool _showLinks = true;
    private bool _showObjectLinks = true;
    
    public override void Init()
    {
        base.Init();
        
        Setup(-1, Image.Load("map.jpg"), new SchemaDto());

        ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
    }
    
    public void Setup(int schemaId, Image image, SchemaDto schemaDto)
    {
        _schemeId = schemaId;
        _schemeName = schemaDto.Name!;
        _imageSize = new Size(image.Size.Width, image.Size.Height);
        Overlay.RemoveImage("map");
        Overlay.AddOrGetImagePointer("map", image.CloneAs<Rgba32>(), false, out _ptr);

        _nextObjectId = -1;
        _nextNodeId = -1;
        _nextLinkId = -1;

        _zoom = 1;
        _imageOffset = Vector2.Zero;
        
        _nodes = schemaDto.Nodes;
        _objects = schemaDto.Objects;
        _links = schemaDto.Links;
        _objectLinks = schemaDto.ObjectLinks;
    }

    public async void FetchAndOpen(int schemeId)
    {
        using var _ = State.BeginDisableScope();
        try
        {
            var schema = await Service<NetService>().GetScheme(schemeId).ValueOrThrow();

            var imageStream = await Service<NetService>().GetImage(schema.ImageUrl!);

            var image = await Image.LoadAsync(imageStream);
            
            Windows.Get<MapEditorView>().Setup(schemeId, image, schema);
            Windows.Open<MapEditorView>();

        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private List<Vector2> dots = new List<Vector2>();

    private const float ImageViewPercent = 0.85f;

    public override void Render()
    {
        if(!IsOpen) return;
        if (!ImGui.Begin("Map",  ref IsOpen)) return;
        
        var drawList = ImGui.GetWindowDrawList();
        
        _winRect.Position = drawList.GetClipRectMin();
        _winRect.Size = new Size(drawList.GetClipRectMax() - _winRect.Position);
            
        _imgView = new Rect(_winRect.Position, new Size(_winRect.Width * ImageViewPercent, _winRect.Height));
        _sideRect = new Rect(_winRect.Position + new Vector2(_imgView.Width, 0),
            new Size(_winRect.Width * (1f - ImageViewPercent), _winRect.Height));
            

        ImGui.PushClipRect(_imgView.Position, _imgView.BottomRight, true);

        if (_ptr != IntPtr.Zero)
        {
            RenderImageRect();
            Editor();
        }
            
        ImGui.PopClipRect();
        ImGui.PushClipRect(_sideRect.Position, _sideRect.BottomRight, true);
        ImGui.SetCursorScreenPos(_sideRect.Position + new Vector2(5,5));
        ImGui.BeginGroup();
        RenderSide();
        ImGui.EndGroup();
            
        ImGui.PopClipRect();
        ImGui.End();
    }

    private void RenderSide()
    {
        ImGui.Text("Visibility: ");
        ImGui.NewLine();
        ImGui.Text("Objects: "); ImGui.SameLine();
        ImGui.Checkbox("##objects_vis", ref _showObjects);
        ImGui.Text("Nodes: "); ImGui.SameLine();
        ImGui.Checkbox("##nodes_vis", ref _showNodes);
        ImGui.Text("Links: "); ImGui.SameLine();
        ImGui.Checkbox("##links_vis", ref _showLinks);
        ImGui.Text("Object Links: "); ImGui.SameLine();
        ImGui.Checkbox("##olinks_vis", ref _showObjectLinks);
        
        ImGui.NewLine();
        ImGui.Text("Name: "); ImGui.SameLine();
        ImGui.InputText("##name", ref _schemeName, 128);
        
        ImGui.NewLine();
        if (ImGui.Button("Save")) OnSave();
        if (ImGui.Button("Reset")) OnReload();
    }

    private void InvalidateScheme()
    {
        _links.RemoveAll(l =>
        {
            if (!HasNode(l.NodeAId)) return true;
            if (!HasNode(l.NodeBId)) return true;

            if (HasCopy(l.NodeBId, l.NodeAId)) return true;
            
            return false;
        });

        bool HasNode(int id) => _nodes.Any(x => x.Id == id);
        bool HasObject(int id) => _objects.Any(x => x.Id == id);
        
        bool HasCopy(int a, int b) => _links.Any(x => x.NodeAId == a && x.NodeBId == b && a < b);
    }

    private async void OnSave()
    {
        using var _ = State.BeginDisableScope();
        try
        {
            InvalidateScheme();
            await Service<NetService>().SaveScheme(new SchemaDto()
            {
                Id = _schemeId,
                
                Nodes = _nodes.ToList(),
                Objects = _objects.ToList(),
                Links = _links.ToList(),
                ObjectLinks = _objectLinks.ToList(),
                Name = _schemeName
                
                
            }).ValueOrThrow();
            
            Windows.Get<MapEditorView>().FetchAndOpen(_schemeId);

        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }
    
    private async void OnReload()
    {
        using var _ = State.BeginDisableScope();
        try
        {
            Windows.Get<MapEditorView>().FetchAndOpen(_schemeId);

        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private void Editor()
    {
        _mouseScreenPosition = ImGui.GetMousePos();
        _mouseImagePosition = null;

        _entityUnderMouse = null;
        
        if (_imgRect.Contains(_mouseScreenPosition))
        {
            _mouseImagePosition = PointToLocal(_mouseScreenPosition);

            if (_showNodes) 
                _entityUnderMouse = GetNodeUnderMouse();
            if (_showObjectLinks &&  _entityUnderMouse is null) 
                _entityUnderMouse = GetObjectLinkUnderMouse();
            if (_showLinks &&  _entityUnderMouse is null) 
                _entityUnderMouse = GetLinkUnderMouse();
            if (_showObjects &&  _entityUnderMouse is null) 
                _entityUnderMouse = GetObjectUnderMouse();
        }
        
        if(_showObjects) Objects();
        if(_showLinks) Links();
        if(_showObjectLinks) ObjectLinks();
        if(_showNodes) Nodes();

        ImageMoving();
        CreateLinkProcess();

        if (!_linkCreation)
        {
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                if (_currentSelectedArea is { })
                {
                    ImGui.OpenPopup("##create_obj_menu");
            
                    _selectedArea = _currentSelectedArea;
                    _currentSelectedArea = null;

                    _newObjectName = "New Object";
                }
                else if (_entityUnderMouse is not null)
                {
                    _contextMenuTarget = _entityUnderMouse;
                    ImGui.OpenPopup("##context_menu");

                    if (_contextMenuTarget is MapObjectDto obj)
                    {
                        _newObjectName = obj.Name;
                    }
                    
                }
                else
                {
                    ImGui.OpenPopup("##create_node_menu");
                }
            }

            DragObject();
            ContextPopup();
            CreateObjectPopup();
            CreateNodePopup();
        }
        
    }
    
    private void DragObject()
    {
        if(ImGui.IsMouseDown(ImGuiMouseButton.Middle) || ImGui.IsMouseDown(ImGuiMouseButton.Right)) return;

        var borderInteractRadius = ScaleToScreen(ObjectCornerInteractRadius);
        var mouseX = _mouseScreenPosition.X;
        var mouseY = _mouseScreenPosition.Y;
        
        if (_entityUnderMouse is MapObjectDto obj1)
        {
            var corner = PointToScreen(obj1.Position.Vector2()) + VectorToScreen(obj1.Size.Vector2());
            if (Vector2.Distance(corner, _mouseScreenPosition) <= borderInteractRadius)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNWSE);
            }
        }

        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            if (_entityUnderMouse is MapObjectDto obj)
            {
                _draggedEntity = obj;
                _dragType = ObjectDragType.MoveObject;

                var corner = PointToScreen(obj.Position.Vector2()) + VectorToScreen(obj.Size.Vector2());
                if (Vector2.Distance(corner, _mouseScreenPosition) <= borderInteractRadius)
                    _dragType = ObjectDragType.Corner;
            }

            if (_entityUnderMouse is NodeDto nodeDto)
            {
                _draggedEntity = nodeDto;
                _dragType = ObjectDragType.MoveObject;
            }
        }
        
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left, 2f))
        {
            var mouseDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 2f);

            var moveDelta = mouseDelta - _lastDragDelta;
            var localDelta = VectorToLocal(moveDelta);

            
            if (_draggedEntity is MapObjectDto obj)
            {
                if (_dragType == ObjectDragType.Corner)
                {
                    obj.Size = new SizeDto(obj.Size.Vector2() + localDelta);
                }
                else
                {
                    obj.Position = new Vector2Dto(obj.Position.Vector2() + localDelta);
                }
            }
            else if (_draggedEntity is NodeDto nodeDto)
            {
                nodeDto.Position = new Vector2Dto(nodeDto.Position.Vector2() + localDelta);
            } 
            
            
            _lastDragDelta = mouseDelta;
        }

        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            _lastDragDelta = Vector2.Zero;
            _draggedEntity = null;
        }
    }

    private MapObjectDto? GetObjectUnderMouse()
    {
        return _objects.LastOrDefault(o => new Rect
        {
            Position = PointToScreen(new Vector2(o.Position.X, o.Position.Y)),
            Size = new Size(VectorToScreen(new Vector2(o.Size.Width, o.Size.Height)))
        }.Contains(_mouseScreenPosition));
    }
    
    private NodeDto? GetNodeUnderMouse()
    {
        return _nodes.LastOrDefault(o => Vector2.Distance(PointToScreen(new Vector2(o.Position.X, o.Position.Y)), _mouseScreenPosition) < ScaleToScreen(NodeInteractRadius));
    }
    
    private LinkDto? GetLinkUnderMouse()
    {
        return _links.LastOrDefault(l =>
        {
            var a = _nodes.FirstOrDefault(x => x.Id == l.NodeAId);
            if (a is null) return false;
            var b = _nodes.FirstOrDefault(x => x.Id == l.NodeBId);
            if (b is null) return false;

            var aScreen = PointToScreen(new Vector2(a.Position.X, a.Position.Y));
            var bScreen = PointToScreen(new Vector2(b.Position.X, b.Position.Y));

            return F.DistanceToSegment(aScreen, bScreen, _mouseScreenPosition) < ScaleToScreen(LinkInteractRadius);
        });
    }
    
    private ObjectLinkDto? GetObjectLinkUnderMouse()
    {
        return _objectLinks.LastOrDefault(l =>
        {
            var node = _nodes.FirstOrDefault(x => x.Id == l.NodeId);
            if (node is null) return false;
            var obj = _objects.FirstOrDefault(x => x.Id == l.ObjectId);
            if (obj is null) return false;

            var nodeScreen = PointToScreen(new Vector2(node.Position.X, node.Position.Y));
            var objScreen = PointToScreen(new Vector2(obj.Position.X + (obj.Size.Width / 2), obj.Position.Y + (obj.Size.Height / 2)));

            return F.DistanceToSegment(nodeScreen, objScreen, _mouseScreenPosition) < ScaleToScreen(LinkInteractRadius);
        });
    }

    private void ImageMoving()
    {
        var wheel = ImGui.GetIO().MouseWheel;

        if (wheel != 0 && _imgView.Contains(_mouseScreenPosition))
        {
            _zoom += wheel * 0.05f;
        }
        
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle, 2f))
        {
            var delta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Middle, 2f);

            _imageOffset += (delta - _lastMiddleDelta);
            _lastMiddleDelta = delta;
        }

        if (ImGui.IsMouseReleased(ImGuiMouseButton.Middle))
        {
            _lastMiddleDelta = Vector2.Zero;
        }
    }

    private void ContextPopup()
    {
        if (ImGui.BeginPopup("##context_menu") && _contextMenuTarget is not null)
        {

            if (_contextMenuTarget is MapObjectDto obj)
            {
                ImGui.InputText("##rename", ref _newObjectName, 128);
                if (ImGui.Button("Save"))
                {
                    obj.Name = _newObjectName;
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.Button("Delete"))
                {
                    _objects.Remove(obj);
                    ImGui.CloseCurrentPopup();
                }
            }
            if (_contextMenuTarget is NodeDto node)
            {
                if (ImGui.Button("CreateLink"))
                {
                    _linkCreation = true;
                    _linkedNode = node;
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.Button("Delete"))
                {
                    _nodes.Remove(node);
                    ImGui.CloseCurrentPopup();
                }
            }
            
            if (_contextMenuTarget is LinkDto link)
            {
                if (ImGui.Button("Delete"))
                {
                    _links.Remove(link);
                    ImGui.CloseCurrentPopup();
                }
            }
            
            if (_contextMenuTarget is ObjectLinkDto olink)
            {
                if (ImGui.Button("Delete"))
                {
                    _objectLinks.Remove(olink);
                    ImGui.CloseCurrentPopup();
                }
            }
            
            ImGui.EndPopup();
        }
    }
    
    private void CreateLinkProcess()
    {
        if (!_linkCreation) return;

        if (_linkedNode is null)
        {
            _linkCreation = false;
            return;
        }
        
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            _linkCreation = false;
            _linkedNode = null;
            return;
        }
        
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            if (_entityUnderMouse is MapObjectDto obj)
            {
                if (!_objectLinks.Any(x => x.NodeId == _linkedNode.Id && x.ObjectId == obj.Id))
                {
                    _objectLinks.Add(new ObjectLinkDto(_nextObjectId--, _linkedNode.Id, obj.Id, _schemeId));
                    _linkCreation = false;
                    _linkedNode = null;
                    return;
                }
                
            }
            if (_entityUnderMouse is NodeDto node)
            {
                if (!_links.Any(x => (x.NodeAId == _linkedNode.Id && x.NodeBId == node.Id) || (x.NodeBId == _linkedNode.Id && x.NodeAId == node.Id)))
                {
                    _links.Add(new LinkDto(_nextLinkId--, _linkedNode.Id, node.Id, _schemeId));
                    _linkCreation = false;
                    _linkedNode = null;
                    return;
                }
                
            }
        }

        var startScreenPos = PointToScreen(new Vector2(_linkedNode.Position.X, _linkedNode.Position.Y));
        
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddLine(startScreenPos,_mouseScreenPosition, LinkCreationColor, ScaleToScreen(LinkThickness));
    }
    
    private void CreateObjectPopup()
    {
        var drawList = ImGui.GetWindowDrawList();
        
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Right, 5f))
        {
            var delta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Right, 5f);

            var b = _mouseScreenPosition;
            var a = b - delta;

            var start = new Vector2(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y));
            var end = new Vector2(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y));

            _currentSelectedArea = new Rect(start, new Size(end - start));
            
            drawList.AddRectFilled(start, end, F.Color(1f,0,0,.5f));
        }
        
        if (!ImGui.BeginPopup("##create_obj_menu") || _selectedArea is null) return;
        if (_selectedArea is {} rect)
        {
            

            drawList.AddRectFilled(rect.Position, rect.BottomRight, F.Color(1f,0,0,.5f));
            
            ImGui.TextUnformatted("New Object");
            ImGui.TextUnformatted("Name: "); ImGui.SameLine();
            ImGui.InputText("##_new_objectName",ref _newObjectName, 128);
            if (ImGui.Button("Create"))
            {
                var localPos = PointToLocal(new Vector2(rect.X, rect.Y));
                var localSize = VectorToLocal(new Vector2(rect.Width, rect.Height));

                _objects.Add(new MapObjectDto(_nextObjectId--, _newObjectName, new Vector2Dto(localPos.X, localPos.Y), new SizeDto(localSize.X, localSize.Y), _schemeId));
                ImGui.CloseCurrentPopup();
            }
        }
            
        ImGui.EndPopup();
    }
    
    private void CreateNodePopup()
    {
        var drawList = ImGui.GetWindowDrawList();

        
        if (!ImGui.BeginPopup("##create_node_menu")) return;
        var popupPos = ImGui.GetMousePosOnOpeningCurrentPopup();
        
        drawList.AddCircle(popupPos, ScaleToScreen(NodeImageRadius), NodeStrokeColor, 0,ScaleToScreen(NodeStrokeThickness));

        if (ImGui.Button("Create Node"))
        {
            var localPos = PointToLocal(popupPos);
                
            _nodes.Add(new NodeDto(_nextNodeId--, new Vector2Dto(localPos.X, localPos.Y), _schemeId));
            ImGui.CloseCurrentPopup();
        }
            
        ImGui.EndPopup();
    }

    private void Objects()
    {
        var drawList = ImGui.GetWindowDrawList();
        
        foreach (var obj in _objects)
        {
            var screenSize = VectorToScreen(new Vector2(obj.Size.Width, obj.Size.Height));
            var screenPos = PointToScreen(new Vector2(obj.Position.X, obj.Position.Y));

            var fillColor = ReferenceEquals(obj, _entityUnderMouse) ? ObjectFillColorHovered : ObjectFillColor;

            var end = screenPos + screenSize;

            drawList.AddRectFilled(screenPos, end, fillColor);
            drawList.AddRect(screenPos, end, ObjectStrokeColor, 0, ImDrawFlags.RoundCornersNone, ScaleToScreen(ObjectStrokeThickness));

            var textSize = ImGui.CalcTextSize(obj.Name, screenSize.X - ObjectNamePadding);

            var intend = (screenSize - textSize) / 2f;
            if (intend.X < 0) intend.X = 0;
            if (intend.Y < 0) intend.Y = 0;

            var textPos = screenPos + intend;

            drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize(), textPos, F.Color(255, 255, 255), obj.Name, screenSize.X - ObjectNamePadding);
        }
    }
    
    private void Nodes()
    {
        var drawList = ImGui.GetWindowDrawList();
        
        foreach (var obj in _nodes)
        {
            var screenPos = PointToScreen(new Vector2(obj.Position.X, obj.Position.Y));

            var fillColor = ReferenceEquals(obj, _entityUnderMouse) ? NodeFillColorHovered : NodeFillColor;

            drawList.AddCircleFilled(screenPos, ScaleToScreen(NodeImageRadius), fillColor);
            drawList.AddCircle(screenPos, ScaleToScreen(NodeImageRadius), NodeStrokeColor, 0,ScaleToScreen(NodeStrokeThickness));
        }
    }

    private void Links()
    {
        var drawList = ImGui.GetWindowDrawList();
        
        foreach (var link in _links)
        {
            var a = _nodes.FirstOrDefault(x => x.Id == link.NodeAId);
            if(a is null) continue;
            var b = _nodes.FirstOrDefault(x => x.Id == link.NodeBId);
            if(b is null) continue;
            

            var color = ReferenceEquals(link, _entityUnderMouse) ? LinkColorHovered : LinkColor;

            drawList.AddLine(PointToScreen(new Vector2(a.Position.X, a.Position.Y)),PointToScreen(new Vector2(b.Position.X, b.Position.Y)), color, ScaleToScreen(LinkThickness));
        }
        
        
    }

    private void ObjectLinks()
    {
        var drawList = ImGui.GetWindowDrawList();

        foreach (var link in _objectLinks)
        {
            var node = _nodes.FirstOrDefault(x => x.Id == link.NodeId);
            if(node is null) continue;
            var obj = _objects.FirstOrDefault(x => x.Id == link.ObjectId);
            if(obj is null) continue;

            var nodePos = new Vector2(node.Position.X, node.Position.Y);
            var objPos = new Vector2(obj.Position.X + (obj.Size.Width / 2), obj.Position.Y + (obj.Size.Height / 2));
            

            var color = ReferenceEquals(link, _entityUnderMouse) ? LinkObjectColorHovered : LinkObjectColor;

            drawList.AddLine(PointToScreen(nodePos),PointToScreen(objPos), color, ScaleToScreen(LinkThickness));
        }
    }
    
    private void RenderImageRect()
    {
        var drawList = ImGui.GetWindowDrawList();
        
        FillRectSizeAndPos(_imgView, _imageSize, out _imgRect);
        _imgRect.Size *= _zoom;
        _imgRect.Position += _imageOffset;
        
        _image2screen = (_imgRect.Size / _imageSize).ToVector2();
        
        drawList.AddImage(_ptr,_imgRect.TopLeft, _imgRect.BottomRight);
    }
    
    private static void FillRectSizeAndPos(Rect viewRect, Size sourceSize, out Rect result)
    {
        result = new Rect
        {
            Size = sourceSize
        };

        result.Size *= (viewRect.Size.Width / result.Size.Width);

        if (result.Size.Height > viewRect.Size.Height)
        {
            result.Size *= (viewRect.Size.Height / result.Size.Height);
        }

        
        result.Position = viewRect.Position + ((viewRect.Size - result.Size) / 2f).ToVector2();
    }
    
    private Vector2 PointToScreen(Vector2 pos) => (pos * _image2screen) + _imgRect.Position;
    private Vector2 PointToLocal(Vector2 pos) => (pos - _imgRect.Position) / _image2screen;
    private Vector2 VectorToScreen(Vector2 v) => v * _image2screen;
    private Vector2 VectorToLocal(Vector2 v) => v / _image2screen;
    private float ScaleToScreen(float value) => _image2screen.Length() * value;

    
}

