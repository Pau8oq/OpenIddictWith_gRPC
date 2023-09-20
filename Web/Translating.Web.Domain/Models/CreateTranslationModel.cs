using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translating.Web.Domain.Models
{
    public record class CreateTranslationModel
    {
        public string EnglishWord { get; init; }
        public string TranslationText { get; init; }
        public LanguageModel Language { get; init; } = new LanguageModel();
    }
}
