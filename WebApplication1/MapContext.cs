using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1;

public sealed class MapContext : DbContext
{
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Schema> Schemas { get; set; } = null!;
    public DbSet<MapNode> Nodes { get; set; } = null!;
    public DbSet<NodeLink> NodeLinks { get; set; } = null!;
    public DbSet<NodeObjectLink> NodeObjectLinks { get; set; } = null!;
    public DbSet<ObjectInstance> Objects { get; set; } = null!;

    public MapContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    

    
}

