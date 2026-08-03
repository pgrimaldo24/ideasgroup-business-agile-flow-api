using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgileFlow.Infrastructure.Persistence.Configurations;

public class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.ToTable("board_columns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(120).IsRequired();

        builder.Property(c => c.Position).HasColumnName("position").HasColumnType("numeric(18,6)").IsRequired();

        builder.Property(c => c.ProjectId).HasColumnName("project_id").IsRequired();

        builder.HasIndex(c => new { c.ProjectId, c.Position });
    }
}
