using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .HasConversion(v => v.Value, v => AssignmentTitle.Create(v).Value)
            .IsRequired();

        builder.Property(a => a.Deadline)
            .IsRequired();

        builder.HasOne<SubjectAggregate>()
            .WithMany(s => s.Assignments)
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
