namespace MapShared.Dto;

public class ObjectLinkDto
{
    public int Id { get; set; }
    public int NodeId { get; set; }
    public int ObjectId { get; set; }
    public int SchemaId { get; set; }

    public ObjectLinkDto() { }
    
    public ObjectLinkDto(int id, int nodeId, int objectId, int schemaId)
    {
        Id = id;
        NodeId = nodeId;
        ObjectId = objectId;
        SchemaId = schemaId;
    }
}

