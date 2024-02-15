using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(Id))]
public class MapNode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    
    [Key]
    public required int SchemaId { get; set; }
    public Schema Schema { get; set; } = null!;
}

