using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<StudentAggregate>
{
    public void Configure(EntityTypeBuilder<StudentAggregate> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasConversion(s => s.Value, v => StudentName.Create(v).Value)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Group)
            .HasConversion(g => g.Value, v => GroupName.Create(v).Value)
            .HasMaxLength(20)
            .HasColumnType("citext")
            .IsRequired();

        builder.Property(s => s.Email)
            .HasConversion(e => e.Value, v => EmailAddress.Create(v).Value)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(s => s.Email).IsUnique();
    }
}
