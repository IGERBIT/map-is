using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(Id))]
public class NodeObjectLink
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Key]
    public int ObjectId { get; set; }
    public int NodeId { get; set; }
    
    public ObjectInstance Object { get; set; } = null!;
    public MapNode Node { get; set; } = null!;
    
    public int SchemaId { get; set; }
    public Schema Schema { get; set; } = null!;
}

