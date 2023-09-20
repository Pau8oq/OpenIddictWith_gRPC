using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translating.Web.Domain.Models;
using Translating.Web.Infrastructure.v1;

namespace Translating.Web.Infrastructure.Mappers
{
    public static class TranslationModelMapper
    {
        public static TranslationModel ToDomain(this TranslationReply translationReply)
        {
            if (translationReply == null)
                return null;

            return new TranslationModel
            {
                Id = translationReply.Id,
                EnglishWord = translationReply.EnglishWord,
                TranslationText = translationReply.TranslationText,
                TranslationLanguage = new LanguageModel() { Name = translationReply.Language.LangName }
            };
        }
    }
}
