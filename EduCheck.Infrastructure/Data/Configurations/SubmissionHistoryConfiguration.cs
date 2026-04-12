using EduCheck.Core.Entities;
using EduCheck.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubmissionHistoryConfiguration : IEntityTypeConfiguration<SubmissionHistory>
{
    public void Configure(EntityTypeBuilder<SubmissionHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.HasIndex(h => h.FileHash);

        builder.Property(h => h.Id)
            .ValueGeneratedOnAdd();

        builder.Property(h => h.FileHash)
            .HasConversion(v => v.Value, v => new FileHash(v))
            .HasMaxLength(128)
            .IsRequired();

        builder.HasOne<Submission>()
            .WithMany(s => s.History)
            .HasForeignKey(h => h.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(h => h.FileName).HasMaxLength(255).IsRequired();
        builder.Property(h => h.FileStoragePath).HasMaxLength(500).IsRequired();
    }
}