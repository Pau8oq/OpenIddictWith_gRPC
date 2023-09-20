using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translating.Domain.AggregatesModel.TranslationAggregate;

namespace Translating.Infrastructure.EntityConfigurations;

public class TranslationEntityTypeConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder.ToTable("Translations", TranslationContext.DEFAULT_SCHEMA);

        builder.HasKey(s=>s.Id);

        builder.Property(s=>s.Id)
                .UseHiLo("translationseq", TranslationContext.DEFAULT_SCHEMA);

        builder.Property<string>("EnglishWord")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("EnglishWord")
            .IsRequired(true);

        builder.Property<string>("TranslationText")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("TranslationText")
            .IsRequired(true);

        builder.Property<string>("TranslationText")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("TranslationText")
            .IsRequired(true);

        builder.HasOne(s=>s.TranslationLanguage)
                .WithMany(s=>s.Translations)
                .HasForeignKey("LanguageId")
                .IsRequired(true); 
    }
}