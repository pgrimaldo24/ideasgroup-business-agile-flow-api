using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgileFlow.Infrastructure.Persistence.Configurations;

public class KanbanTaskConfiguration : IEntityTypeConfiguration<KanbanTask>
{
    public void Configure(EntityTypeBuilder<KanbanTask> builder)
    {
        builder.ToTable("kanban_tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(4000);
        builder.Property(t => t.Priority).HasColumnName("priority").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.AssigneeName).HasColumnName("assignee_name").HasMaxLength(200);
        builder.Property(t => t.ColumnId).HasColumnName("column_id").IsRequired();

        builder.Property(t => t.Position).HasColumnName("position").HasColumnType("numeric(18,6)").IsRequired();

        builder.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(t => new { t.ColumnId, t.Position });

        builder.HasOne<BoardColumn>()
            .WithMany()
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
