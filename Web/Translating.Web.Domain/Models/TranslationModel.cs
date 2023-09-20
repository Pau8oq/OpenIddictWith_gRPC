using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translating.Web.Domain.Models
{
    public record class TranslationModel
    {
        public int Id { get; init; }
        public string EnglishWord { get; init; }
        public string TranslationText { get; init; }
        public LanguageModel TranslationLanguage { get; init; }
    }
}
