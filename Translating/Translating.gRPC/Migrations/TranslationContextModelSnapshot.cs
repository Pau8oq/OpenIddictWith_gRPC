﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Translating.Infrastructure;

#nullable disable

namespace Translating.gRPC.Migrations
{
    [DbContext(typeof(TranslationContext))]
    partial class TranslationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.HasSequence("languageseq", "translation")
                .IncrementsBy(10);

            modelBuilder.HasSequence("translationseq", "translation")
                .IncrementsBy(10);

            modelBuilder.Entity("Translating.Domain.AggregatesModel.TranslationAggregate.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseHiLo(b.Property<int>("Id"), "languageseq", "translation");

                    b.Property<string>("LangName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("LangName");

                    b.HasKey("Id");

                    b.ToTable("Languages", "translation");
                });

            modelBuilder.Entity("Translating.Domain.AggregatesModel.TranslationAggregate.Translation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseHiLo(b.Property<int>("Id"), "translationseq", "translation");

                    b.Property<string>("EnglishWord")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("EnglishWord");

                    b.Property<int>("LanguageId")
                        .HasColumnType("int");

                    b.Property<string>("TranslationText")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("TranslationText");

                    b.HasKey("Id");

                    b.HasIndex("LanguageId");

                    b.ToTable("Translations", "translation");
                });

            modelBuilder.Entity("Translating.Domain.AggregatesModel.TranslationAggregate.Translation", b =>
                {
                    b.HasOne("Translating.Domain.AggregatesModel.TranslationAggregate.Language", "TranslationLanguage")
                        .WithMany("Translations")
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TranslationLanguage");
                });

            modelBuilder.Entity("Translating.Domain.AggregatesModel.TranslationAggregate.Language", b =>
                {
                    b.Navigation("Translations");
                });
#pragma warning restore 612, 618
        }
    }
}
