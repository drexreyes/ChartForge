using ChartForge.Core.Entities;
using ChartForge.Infrastructure.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ChartForge.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ChartState> ChartState { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.ChartState)
            .WithOne(cs => cs.Message)
            .HasForeignKey<Message>(m => m.ChartStateId);
    }
}