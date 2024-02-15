using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(Id))]
public class Organization
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Site { get; set; } = null!;
    public string Address { get; set; } = null!;
    
    
    public ICollection<Schema> Schemas { get; } = new List<Schema>();
    public ICollection<Member> Members { get; } = new List<Member>(); 

}

