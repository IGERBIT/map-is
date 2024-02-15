namespace MapShared.Dto;

public class NodeDto
{
    public NodeDto(int id, Vector2Dto position, int schemaId)
    {
        Id = id;
        Position = position;
        SchemaId = schemaId;
    }
    
    public int Id { get; set; }
    public Vector2Dto Position { get; set; }
    public int SchemaId { get; set; }
    
    public void Deconstruct(out int id, out Vector2Dto position, out int schemaId)
    {
        id = Id;
        position = Position;
        schemaId = SchemaId;
    }
}

