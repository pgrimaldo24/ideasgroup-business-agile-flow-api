using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgileFlow.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(p => p.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(p => p.ExpectedEndDate).HasColumnName("expected_end_date").IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasMany(p => p.Columns)
            .WithOne()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Project.Columns))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => p.Name);
    }
}
