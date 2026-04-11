using EduCheck.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCheck.Infrastructure.Data.Configurations;

public class SubjectTargetGroupConfiguration : IEntityTypeConfiguration<SubjectTargetGroup>
{
    public void Configure(EntityTypeBuilder<SubjectTargetGroup> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Subject)
            .WithMany(s => s.TargetGroups)
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}