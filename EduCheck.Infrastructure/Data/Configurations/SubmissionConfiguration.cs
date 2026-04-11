using EduCheck.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.StudentId, s.AssignmentId }).IsUnique();

        builder.HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId);

        builder.HasOne(s => s.Assignment)
            .WithMany()
            .HasForeignKey(s => s.AssignmentId);

        builder.HasMany(s => s.History)
            .WithOne()
            .HasForeignKey(h => h.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
