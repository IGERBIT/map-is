using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(Id))]
public class ObjectGroupAssignment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Key]
    public required int InstanceId { get; set; }
    [Key]
    public required int GroupId { get; set; }

    public ObjectInstance Instance { get; set; } = null!;
    public ObjectGroup Group { get; set; } = null!;
}

