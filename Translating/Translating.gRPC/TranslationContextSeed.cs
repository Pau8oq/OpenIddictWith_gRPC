using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Polly;
using System.Net;
using Translating.Domain.AggregatesModel.TranslationAggregate;
using Translating.Infrastructure;

namespace Translating.gRPC
{
    public static class TranslationContextSeed
    {
        public static async void ApplyMigrationAndSeed(this IApplicationBuilder app)
        {
            var retry = Policy.Handle<SqlException>()
                .WaitAndRetry(new TimeSpan[]
                {
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(8)
                });

            await retry.Execute(async () => await InvokeSeedOrder(app));
        }

        private static async Task InvokeSeedOrder(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var context = scope.ServiceProvider.GetService<TranslationContext>();
            //we do not use approach below .If you want to enable migrations
            context.Database.Migrate();
            await SeedAsync(context);
        }

        private static async Task SeedAsync(TranslationContext context)
        {
            if (!context.Translations.Any())
            {
                context.Translations.AddRange(GetPredefinedTranslations());
                await context.SaveChangesAsync();
            }
        }

        private static IEnumerable<Translation> GetPredefinedTranslations()
        {
            return new List<Translation>()
            {
                new Translation("yellow", "жовтий", new Language("UA"))
            };
        }
    }
}
