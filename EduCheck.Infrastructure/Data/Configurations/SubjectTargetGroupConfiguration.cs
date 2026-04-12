using EduCheck.Core.Entities;
using EduCheck.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubjectTargetGroupConfiguration : IEntityTypeConfiguration<SubjectTargetGroup>
{
    public void Configure(EntityTypeBuilder<SubjectTargetGroup> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(tg => tg.Id)
            .ValueGeneratedOnAdd();

        builder.Property(tg => tg.GroupName)
            .HasConversion(v => v.Value, v => new GroupName(v))
            .HasColumnType("citext")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne<Subject>()
            .WithMany(s => s.TargetGroups)
            .HasForeignKey(tg => tg.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}