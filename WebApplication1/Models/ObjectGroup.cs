using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(Id))]
public class ObjectGroup
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    
    [Key]
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}

