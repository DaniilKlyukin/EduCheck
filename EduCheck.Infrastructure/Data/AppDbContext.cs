using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace EduCheck.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<StudentAggregate> Students => Set<StudentAggregate>();
    public DbSet<SubjectAggregate> Subjects => Set<SubjectAggregate>();
    public DbSet<SubmissionAggregate> Submissions => Set<SubmissionAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var mediator = this.GetService<IMediator>();

        var aggregateRoots = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var domainEvents = aggregateRoots.SelectMany(e => e.DomainEvents).ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (mediator != null)
        {
            foreach (var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent, cancellationToken);
            }
        }

        aggregateRoots.ForEach(e => e.ClearDomainEvents());
        return result;
    }
}