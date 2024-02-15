using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(Id))]
public class Schema
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ImageName { get; set; } = null!;
    
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public ICollection<MapNode> Nodes { get; } = new List<MapNode>(); 
    public ICollection<ObjectInstance> Objects { get; } = new List<ObjectInstance>(); 
    public ICollection<NodeLink> Links { get; } = new List<NodeLink>(); 
    public ICollection<NodeObjectLink> ObjectLinks { get; } = new List<NodeObjectLink>(); 
    
}

