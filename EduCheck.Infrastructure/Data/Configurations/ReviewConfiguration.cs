using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Grade)
            .HasConversion(
                v => v == null ? (int?)null : v.Value,
                v => v == null ? null : Grade.Create(v.Value).Value)
            .IsRequired(false);

        builder.Property(r => r.SubmissionVersion)
            .HasConversion(
                v => v.Value,
                v => SubmissionVersion.Create(v).Value)
            .IsRequired();

        builder.Property(r => r.TeacherComment)
            .HasMaxLength(2000);

        builder.Property(r => r.CheckedAt)
            .IsRequired();

        builder.HasOne<SubmissionAggregate>()
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}