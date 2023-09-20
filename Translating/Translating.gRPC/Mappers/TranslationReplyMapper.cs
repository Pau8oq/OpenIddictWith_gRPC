using Translating.Domain.AggregatesModel.TranslationAggregate;
using TranslationService.v1;

namespace Translating.gRPC.Mappers
{
    public static class TranslationReplyMapper
    {
        public static TranslationReply ToReply(this Translation translation)
        {
            if (translation is null)
                return null;

            var countryReply = new TranslationReply
            {
                Id = translation.Id,
                EnglishWord = translation.GetEnglishWord(),
                TranslationText = translation.GetTranslationText(),
                Language = new TranslationService.v1.Language() { LangName = translation.TranslationLanguage.GetLangName() }
            };

            return countryReply;
        }
    }
}
