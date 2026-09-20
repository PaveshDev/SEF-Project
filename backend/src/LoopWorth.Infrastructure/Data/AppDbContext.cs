using LoopWorth.Domain.Entities;
using LoopWorth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoopWorth.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemImage> ItemImages => Set<ItemImage>();
    public DbSet<ItemAssessment> ItemAssessments => Set<ItemAssessment>();
    public DbSet<AgentWorkflow> AgentWorkflows => Set<AgentWorkflow>();
    public DbSet<AgentWorkflowStep> AgentWorkflowSteps => Set<AgentWorkflowStep>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Category
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Configure Item
        builder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Items)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Map the enum explicitly to string if desired, or let it map to int. Let's map to string for readability.
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.SelectedRecoveryRoute).HasConversion<string>();
        });

        // Configure ItemImage
        builder.Entity<ItemImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Item)
                  .WithMany(i => i.Images)
                  .HasForeignKey(e => e.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ItemAssessment
        builder.Entity<ItemAssessment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Item)
                  .WithMany(i => i.Assessments)
                  .HasForeignKey(e => e.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.ConditionLevel).HasConversion<string>();
            entity.Property(e => e.RecommendedRoute).HasConversion<string>();
            entity.Property(e => e.AlternativeRoute).HasConversion<string>();
            entity.Property(e => e.ConfidenceLevel).HasConversion<string>();
        });

        // Configure AgentWorkflow
        builder.Entity<AgentWorkflow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Item)
                  .WithMany(i => i.Workflows)
                  .HasForeignKey(e => e.ItemId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure AgentWorkflowStep
        builder.Entity<AgentWorkflowStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Workflow)
                  .WithMany(w => w.Steps)
                  .HasForeignKey(e => e.WorkflowId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
