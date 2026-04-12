using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubjectTargetGroupConfiguration : IEntityTypeConfiguration<SubjectTargetGroup>
{
    public void Configure(EntityTypeBuilder<SubjectTargetGroup> builder)
    {
        builder.ToTable("SubjectTargetGroups");
        builder.HasKey(tg => tg.Id);

        builder.Property(tg => tg.GroupName)
            .HasConversion(v => v.Value, v => GroupName.Create(v).Value)
            .HasColumnType("citext")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne<SubjectAggregate>()
            .WithMany(s => s.TargetGroups)
            .HasForeignKey(tg => tg.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}