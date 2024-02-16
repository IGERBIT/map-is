using client.Services;
using ImGuiNET;
using MapShared.Dto;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vortice.Mathematics;
using Size = Vortice.Mathematics.Size;
using Vector2 = System.Numerics.Vector2;


namespace client.Views;

public class MapView : ImGuiWindow
{
    private static readonly uint ObjectFillColor = F.Color(.1f, .1f, 1f, .5f);
    private static readonly uint ObjectStrokeColor = F.Color(0f, 0f, 0f);
    private static readonly float ObjectStrokeThickness = 2;
    private static readonly float ObjectNamePadding = 10f;
    
    
    private static readonly float PathThickness = 3f;
    private static readonly uint PathColor = F.Color(.1f, 1f, .1f, .3f);
    
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

    
    private Vector2 _mouseScreenPosition;
    
    private Vector2 _imageOffset;
    private float _zoom = 1f;
    private Vector2 _lastMiddleDelta;
    
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
        _imageSize = new Size(image.Size.Width, image.Size.Height);
        Overlay.RemoveImage("map");
        Overlay.AddOrGetImagePointer("map", image.CloneAs<Rgba32>(), false, out _ptr);

        _zoom = 1;
        _imageOffset = Vector2.Zero;
        
        _nodes = schemaDto.Nodes;
        _objects = schemaDto.Objects;
        _links = schemaDto.Links;
        _objectLinks = schemaDto.ObjectLinks;

        UpdateItemsFromObjects();
    }

    public async void FetchAndOpen(int schemeId)
    {
        using var _ = State.BeginDisableScope();
        try
        {
            var schema = await Service<NetService>().GetScheme(schemeId).ValueOrThrow();

            var imageStream = await Service<NetService>().GetImage(schema.ImageUrl!);

            var image = await Image.LoadAsync(imageStream);
            
            Windows.Get<MapView>().Setup(schemeId, image, schema);
            Windows.Open<MapView>();

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
            Navigator();
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

    private readonly ListBoxWithSearch _fromList = new ListBoxWithSearch("##from", Array.Empty<string>(), 32, 10);
    private readonly ListBoxWithSearch _toList = new ListBoxWithSearch("##to", Array.Empty<string>(), 32, 10);

    private int _lastObjectFrom = -1;
    private int _lastObjectTo = -1;
    
    private int _lastObjectFromPath = -2;
    private int _lastObjectToPath = -2;

    private MapObjectDto? _fromObject;
    private MapObjectDto? _toObject;

    private List<int> _fromNodes = new List<int>();
    private List<int> _toNodes = new List<int>();
    

    private string[] _objectNames;

    private void UpdateItemsFromObjects()
    {
        _objectNames = _objects.Select(x => x.Name).ToArray();
        _fromList.SetItems(_objectNames);
        _toList.SetItems(_objectNames);
    }

    private void RenderSide()
    {
        ImGui.Text("From: ");
        if (_fromList.Render())
        {
            _lastObjectFrom = _fromList.CurrentItem;
        }
        ImGui.Text("To: ");
        if (_toList.Render())
        {
            _lastObjectTo = _toList.CurrentItem;
        }

        if (_lastObjectFromPath != _lastObjectFrom || _lastObjectToPath != _lastObjectTo)
        {
            _lastObjectFromPath = _lastObjectFrom;
            _lastObjectToPath = _lastObjectTo;
            Recalculate();
        }
        
    }

    private Dictionary<int, float> _weighs = new Dictionary<int, float>();
    private Dictionary<int, int> _parent = new Dictionary<int, int>();
    private Dictionary<int, List<int>> _neighbors = new Dictionary<int, List<int>>();
    private Dictionary<int, Vector2> _poses = new Dictionary<int, Vector2>();
    private List<int> _left = new ();
    private List<int> _visited = new ();
    private List<Vector2> _resultPath = new List<Vector2>();


    private void FillPreCalculateData()
    {
        _weighs.Clear();
        _neighbors.Clear();
        _poses.Clear();
        _parent.Clear();
        _left.Clear();
        _visited.Clear();
        _resultPath.Clear();
        
        foreach (var node in _nodes)
        {
            _weighs[node.Id] = float.PositiveInfinity;
            _neighbors[node.Id] = new List<int>();
            _poses[node.Id] = node.Position.Vector2();
            _left.Add(node.Id);
        }
        
        foreach (var link in _links)
        {
            _neighbors[link.NodeAId].Add(link.NodeBId);
            _neighbors[link.NodeBId].Add(link.NodeAId);
        }
    }

    private void Recalculate()
    {
        _fromObject = _objects.ElementAtOrDefault(_lastObjectFromPath);
        if(_fromObject is null) return;
        _toObject = _objects.ElementAtOrDefault(_lastObjectTo)!;
        if(_toObject is null) return;

        _fromNodes = _objectLinks
            .Where(x => x.ObjectId == _fromObject.Id)
            .Select(x => _nodes.FirstOrDefault(n => n.Id == x.NodeId))
            .Where(x => x is not null).Select(x=> x!.Id)
            .ToList();
        
        _toNodes = _objectLinks
            .Where(x => x.ObjectId == _toObject.Id)
            .Select(x => _nodes.FirstOrDefault(n => n.Id == x.NodeId))
            .Where(x => x is not null).Select(x=> x!.Id)
            .ToList();
        
        if(_fromNodes.Count < 1) return;
        if(_toNodes.Count < 1) return;
        
        FillPreCalculateData();
        
        var queue = new Queue<(int node, float length)>();
        foreach (var node in _fromNodes)
        {
            _weighs[node] = 0;
            queue.Enqueue((node, 0));
        }

        while (queue.TryDequeue(out var item))
        {
            if(item.length > _weighs[item.node]) continue;

            foreach (var neighbor in _neighbors[item.node])
            {
                var distance = Vector2.Distance(_poses[neighbor], _poses[item.node]);
                var newWeight = item.length + distance;

                if (newWeight < _weighs[neighbor])
                {
                    _weighs[neighbor] = newWeight;
                    _parent[neighbor] = item.node;
                    queue.Enqueue((neighbor, newWeight));
                }
            }
        }

        var endNode = -1;
        var minWay = float.PositiveInfinity;
        
        foreach (var node in _toNodes)
        {
            if (!(_weighs[node] < minWay)) continue;
            endNode = node;
            minWay = _weighs[node];
        }

        if (endNode == -1) return;

        var backpath = new List<int>();
        
        backpath.Add(endNode);

        if (!_fromNodes.Contains(endNode))
        {
            var currentBackWayNode = endNode;
            while (_parent.TryGetValue(currentBackWayNode, out var parent))
            {
                backpath.Add(parent);
            
                if(_fromNodes.Contains(parent)) break;
                currentBackWayNode = parent;
            }
        }

        _resultPath = backpath.Select(x => _poses[x]).ToList();
    }
    
    private void Navigator()
    {
        _mouseScreenPosition = ImGui.GetMousePos();
        
        PathRender();

        ImageMoving();
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

    

    private void PathRender()
    {
        var drawList = ImGui.GetWindowDrawList();
       
        //Debug();

        if (_fromObject is not null) DrawObject(_fromObject);
        if (_toObject is not null) DrawObject(_toObject);
        
        if(_resultPath.Count < 1 || _fromObject is null || _toObject is null || _toObject == _fromObject) return;

        var fromCenter = (_fromObject.Position.Vector2() + (_fromObject.Size.Vector2() / 2f));
        var toCenter = (_toObject.Position.Vector2() + (_toObject.Size.Vector2() / 2f));
        var start = _resultPath[0];

        drawList.AddLine(PointToScreen(toCenter),PointToScreen(start), PathColor, ScaleToScreen(PathThickness));
        
        for (var i = 1; i < _resultPath.Count; i++)
        {
            var end = _resultPath[i];
            drawList.AddLine(PointToScreen(start),PointToScreen(end), PathColor, ScaleToScreen(PathThickness));
            start = end;
        }
        drawList.AddLine(PointToScreen(start),PointToScreen(fromCenter), PathColor, ScaleToScreen(PathThickness));
    }

    private void Debug()
    {
        var drawList = ImGui.GetWindowDrawList();
        foreach (var (node, pos) in _poses)
        {
            drawList.AddText(PointToScreen(pos), F.Color(1f, 1f, 1f), _weighs[node].ToString("F3"));
        }

        foreach (var node in _fromNodes)
        {
            drawList.AddCircleFilled(PointToScreen(_poses[node]), ScaleToScreen(5f), F.Color(1f, 1f, 1f, .5f));
        }

        foreach (var node in _toNodes)
        {
            drawList.AddCircleFilled(PointToScreen(_poses[node]), ScaleToScreen(5f), F.Color(0f, 0f, 0f, 0.5f));
        }
    }

    private void DrawObject(MapObjectDto obj)
    {
        var drawList = ImGui.GetWindowDrawList();
        var screenSize = VectorToScreen(new Vector2(obj.Size.Width, obj.Size.Height));
        var screenPos = PointToScreen(new Vector2(obj.Position.X, obj.Position.Y));

        var end = screenPos + screenSize;

        drawList.AddRectFilled(screenPos, end, ObjectFillColor);
        drawList.AddRect(screenPos, end, ObjectStrokeColor, 0, ImDrawFlags.RoundCornersNone, ScaleToScreen(ObjectStrokeThickness));

        var textSize = ImGui.CalcTextSize(obj.Name, screenSize.X - ObjectNamePadding);

        var intend = (screenSize - textSize) / 2f;
        if (intend.X < 0) intend.X = 0;
        if (intend.Y < 0) intend.Y = 0;

        var textPos = screenPos + intend;

        drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize(), textPos, F.Color(255, 255, 255), obj.Name, screenSize.X - ObjectNamePadding);
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

