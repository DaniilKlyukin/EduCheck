using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<SubjectAggregate>
{
    public void Configure(EntityTypeBuilder<SubjectAggregate> builder)
    {
        builder.ToTable("Subjects");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .HasConversion(v => v.Value, v => SubjectTitle.Create(v).Value)
            .HasColumnType("citext")
            .IsRequired();

        builder.Property(s => s.Semester)
            .HasConversion(v => v.Value, v => Semester.Create(v).Value)
            .IsRequired();

        builder.Navigation(s => s.Assignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(s => s.TargetGroups)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Assignments)
            .WithOne()
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.TargetGroups)
            .WithOne()
            .HasForeignKey(tg => tg.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.Title, s.Semester }).IsUnique();
    }
}
