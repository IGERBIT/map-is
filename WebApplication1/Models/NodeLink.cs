using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(Id))]
public class NodeLink
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int NodeAId { get; set; }
    public int NodeBId { get; set; }
    public MapNode NodeA { get; set; } = null!;
    public MapNode NodeB { get; set; } = null!;

    public float WeightFactor { get; set; } = 1f;
    
    public int SchemaId { get; set; }
    public Schema Schema { get; set; } = null!;
}

