using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(Id))]
public class Member
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string FullName { get; set; }  = null!;
    public string Email { get; set; }  = null!;
    public string Password { get; set; } = null!;
    public Role Role { get; set; }
    public DateTime AssignDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpirationDate { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}

