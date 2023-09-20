using Translating.Domain.AggregatesModel.TranslationAggregate;
using Microsoft.EntityFrameworkCore;
using Translating.Infrastructure.EntityConfigurations;

namespace Translating.Infrastructure;

public class TranslationContext: DbContext
{
    public const string DEFAULT_SCHEMA = "translation";
    public DbSet<Translation> Translations{ get; set; }

    public TranslationContext(DbContextOptions<TranslationContext> options): base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TranslationEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new LanguageEntityTypeConfiguration());
    }
}