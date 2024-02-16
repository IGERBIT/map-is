namespace MapShared.Dto;

public class SchemaDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public List<NodeDto> Nodes { get; set; } = new();
    public List<LinkDto> Links { get; set; } = new();
    public List<ObjectLinkDto> ObjectLinks { get; set; } = new();
    public List<MapObjectDto> Objects { get; set; } = new();
}

