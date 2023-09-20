using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translating.Domain.AggregatesModel.TranslationAggregate;

namespace Translating.Infrastructure.EntityConfigurations;

public class LanguageEntityTypeConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages", TranslationContext.DEFAULT_SCHEMA);

        builder.HasKey(s=>s.Id);

        builder.Property(s=>s.Id)
            .UseHiLo("languageseq", TranslationContext.DEFAULT_SCHEMA);

        builder.Property<string>("LangName")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("LangName")
            .IsRequired(true);

        builder.HasMany(s=>s.Translations)
            .WithOne(s=>s.TranslationLanguage)
            .HasForeignKey("LanguageId")
            .IsRequired(true); 
    }
}