using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1;

public sealed class MapContext : DbContext
{
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;

    public MapContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    

    
}

