namespace MapShared.Dto;

public class LinkDto
{
    public int Id { get; set; }
    public int NodeAId { get; set; }
    public int NodeBId { get; set; }
    public int SchemaId { get; set; }
    
    public LinkDto() { }
    
    public LinkDto(int id, int nodeAId, int nodeBId, int schemaId)
    {
        Id = id;
        NodeAId = nodeAId;
        NodeBId = nodeBId;
        SchemaId = schemaId;
    }
    
}

