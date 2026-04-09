using EduCheck.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubmissionHistoryConfiguration : IEntityTypeConfiguration<SubmissionHistory>
{
    public void Configure(EntityTypeBuilder<SubmissionHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.HasIndex(h => h.FileHash);

        builder.Property(h => h.FileName).HasMaxLength(255).IsRequired();
        builder.Property(h => h.FileStoragePath).HasMaxLength(500).IsRequired();
    }
}