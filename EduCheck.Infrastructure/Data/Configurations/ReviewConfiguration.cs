using EduCheck.Core.Entities;
using EduCheck.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Grade)
            .HasConversion(v => v == null ? (int?)null : v.Value, v => v == null ? null : new Grade(v.Value))
            .IsRequired();

        builder.HasOne<Submission>()
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}