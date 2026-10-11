using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.App.Features.Catalogs.Entities;
using TaskManager.App.Features.Improvements.Entities;
using TaskManager.App.Features.Tasks.Entities;
using TaskManager.App.Features.TimeTracking.Entities;

namespace TaskManager.App.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<SubProject> SubProjects => Set<SubProject>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<Improvement> Improvements => Set<Improvement>();
    public DbSet<ImprovementAction> ImprovementActions => Set<ImprovementAction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256);
            e.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<Project>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasOne(x => x.Company).WithMany(x => x.Projects).HasForeignKey(x => x.CompanyId);
        });

        builder.Entity<SubProject>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.PlannedHours).HasPrecision(10, 2);
            e.HasOne(x => x.Project).WithMany(x => x.SubProjects).HasForeignKey(x => x.ProjectId);
        });

        builder.Entity<WorkTask>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(250).IsRequired();
            e.Property(x => x.PlannedHours).HasPrecision(10, 2);
            e.Property(x => x.ActualHours).HasPrecision(10, 2);
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Project).WithMany(x => x.Tasks).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SubProject).WithMany().HasForeignKey(x => x.SubProjectId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.Status, x.DueDate });
            e.HasIndex(x => x.Priority);
        });

        builder.Entity<TimeEntry>(e =>
        {
            e.Property(x => x.Hours).HasPrecision(10, 2);
            e.Property(x => x.HourlyCost).HasPrecision(10, 2);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasOne(x => x.WorkTask).WithMany(x => x.TimeEntries).HasForeignKey(x => x.WorkTaskId);
        });

        builder.Entity<Improvement>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(250).IsRequired();
            e.Property(x => x.ClosureCost).HasPrecision(12, 2);
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ImprovementAction>(e =>
        {
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Improvement).WithMany(x => x.Actions).HasForeignKey(x => x.ImprovementId);
        });
    }
}
