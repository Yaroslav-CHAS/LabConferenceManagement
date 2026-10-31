using ConferenceManagement.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace ConferenceManagement.Infrastructure.Data;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

    public DbSet<Conference> Conferences { get; set; }

    public DbSet<ScientificDirection> ScientificDirections { get; set; }

    public DbSet<Section> Sections { get; set; }

    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conference>()
            .HasMany(x => x.Directions)
            .WithOne(x => x.Conference)
            .HasForeignKey(x => x.ConferenceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScientificDirection>()
            .HasMany(x => x.Sections)
            .WithOne(x => x.ScientificDirection)
            .HasForeignKey(x => x.ScientificDirectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Section>()
            .HasOne(x => x.Chairman)
            .WithMany(x => x.ChairedSections)
            .HasForeignKey(x => x.ChairmanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Section>()
            .HasOne(x => x.Secretary)
            .WithMany(x => x.SecretarySections)
            .HasForeignKey(x => x.SecretaryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ScientificDirection>()
            .HasIndex(x => new { x.ConferenceId, x.Index })
            .IsUnique();

        modelBuilder.Entity<Section>()
            .HasIndex(x => new { x.ScientificDirectionId, x.Index })
            .IsUnique();
    }
}