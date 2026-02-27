using ChartForge.Core.Entities;
using ChartForge.Infrastructure.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ChartForge.Infrastructure.Data;

public class AppDbContext : DbContext
{
    // The constructor accepts DbContextOptions, which carries the connection
    // string and provider information (SQL Server in our case).
    // We never hardcode a connection string here - it is injected from
    // outside, keeping this class environment-agnostic. The same class
    // works in development, staging, and production unchanged.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // --- DbSets ---
    // Each DbSet is our gateway to querying and saving that entity type.
    // EF Core also uses these to discover which types to include in the
    // database model during migration generation.
    public DbSet<User> Users => Set<User>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ChartState> ChartState => Set<ChartState>();

    // OnModelCreating is called once at application startup when EF Core
    // builds its internal schema model. This is where we hand off
    // configuration responsibility to our dedicated configuration classes.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Instead of configuring every entity inline here - which would
        // make this method grow to hundreds of lines - we delegate each
        // entity's configuration to its own dedicated class.
        // ApplyConfiguration registers one entity's complete ruleset.
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new ChartStateConfiguration());
    }
}