using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubmissionHistoryConfiguration : IEntityTypeConfiguration<SubmissionHistory>
{
    public void Configure(EntityTypeBuilder<SubmissionHistory> builder)
    {
        builder.ToTable("SubmissionHistories");
        builder.HasKey(h => h.Id);

        builder.OwnsOne(h => h.File, file =>
        {
            file.Property(f => f.Name)
                .HasColumnName("FileName")
                .HasMaxLength(255)
                .IsRequired();

            file.Property(f => f.StoragePath)
                .HasColumnName("FileStoragePath")
                .HasMaxLength(500)
                .IsRequired();

            file.Property(f => f.Hash)
                .HasColumnName("FileHash")
                .HasConversion(v => v.Value, v => FileHash.Create(v).Value)
                .HasMaxLength(128)
                .IsRequired();

            file.HasIndex(f => f.Hash);
        });

        builder.Property(h => h.AnalysisResult)
            .HasColumnType("jsonb");

        builder.Property(h => h.ReceivedAt).IsRequired();
        builder.Property(h => h.IsLate).IsRequired();

        builder.HasOne<SubmissionAggregate>()
            .WithMany(s => s.History)
            .HasForeignKey(h => h.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}