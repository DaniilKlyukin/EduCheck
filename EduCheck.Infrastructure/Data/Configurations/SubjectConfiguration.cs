using EduCheck.Core.Entities;
using EduCheck.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.Title, s.Semester }).IsUnique();

        builder.Property(s => s.Title)
            .HasConversion(v => v.Value, v => new SubjectTitle(v))
            .HasColumnType("citext")
            .IsRequired();

        builder.Metadata.FindNavigation(nameof(Subject.Assignments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(Subject.TargetGroups))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Assignments)
            .WithOne()
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.TargetGroups)
            .WithOne()
            .HasForeignKey(tg => tg.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
