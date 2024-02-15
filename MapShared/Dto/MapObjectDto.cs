namespace MapShared.Dto;

public class MapObjectDto
{
    
    public MapObjectDto(){}
    
    public MapObjectDto(int id, string name, Vector2Dto position, SizeDto size, int schemaId)
    {
        Id = id;
        Name = name;
        Position = position;
        Size = size;
        SchemaId = schemaId;
    }
    public int Id { get; set; }
    public string Name { get; set; }
    public Vector2Dto Position { get; set; }
    public SizeDto Size { get; set; }
    public int SchemaId { get; set; }
    
}

