using EduCheck.Core.Entities;
using EduCheck.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.Email).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();

        builder.Property(s => s.Group)
            .HasConversion(g => g.Value, g => new GroupName(g))
            .HasMaxLength(20)
            .HasColumnType("citext")
            .IsRequired();

        builder.Property(s => s.Email)
            .HasConversion(e => e.Value, v => new EmailAddress(v))
            .HasMaxLength(150)
            .IsRequired();
    }
}
