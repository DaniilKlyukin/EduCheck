using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<SubmissionAggregate>
{
    public void Configure(EntityTypeBuilder<SubmissionAggregate> builder)
    {
        builder.ToTable("Submissions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.CurrentVersion)
            .HasConversion(
                v => v != null ? v.Value : (int?)null,
                v => v.HasValue ? SubmissionVersion.Create(v.Value).Value : null)
            .IsRequired(false);

        builder.Navigation(s => s.History)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(s => s.Reviews)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => new { s.StudentId, s.AssignmentId }).IsUnique();
        builder.HasIndex(s => s.AssignmentId);
    }
}
