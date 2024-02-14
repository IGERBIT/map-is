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
    public required float X { get; set; }
    public required float Y { get; set; }
    
    [Key]
    public required int SchemeId { get; set; }
    public Schema Schema { get; set; } = null!;
}

