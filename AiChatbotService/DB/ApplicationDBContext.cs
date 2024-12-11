using Microsoft.EntityFrameworkCore;

namespace AiChatbot.DB;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options) { }

    public virtual DbSet<AIChatHistory> AIChatHistory { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.Entity<AIChatHistory>(entity =>
        {
            entity.Property(e => e.recid).HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.itemkey).HasDefaultValueSql("''::text");
            
            entity.Property(e => e.username).HasDefaultValueSql("''::text");
            
            entity.Property(e => e.channel).HasDefaultValueSql("''::text");
            
            entity.Property(e => e.createdateutc).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.lastupdateutc).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.messages).HasDefaultValueSql("'{}'::jsonb");
            
            entity.Property(e => e.logs).HasDefaultValueSql("'{}'::jsonb");
        });
    }
}
