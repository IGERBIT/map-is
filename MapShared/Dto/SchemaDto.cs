namespace MapShared.Dto;

public class SchemaDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public List<NodeDto> Nodes { get; set; }
    public List<LinkDto> Links { get; set; }
    public List<ObjectLinkDto> ObjectLinks { get; set; }
    public List<MapObjectDto> Objects { get; set; }
}

