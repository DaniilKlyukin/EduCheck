using EduCheck.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EduCheck.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<SubmissionHistory> SubmissionHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}