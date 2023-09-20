using Translating.Domain.SeedWork;

namespace Translating.Domain.AggregatesModel.TranslationAggregate;

public class Language: Entity
{
    private string LangName;
    public string GetLangName() => LangName;
    public List<Translation> Translations {get;set;}
    public Language()
    {
        Translations = new List<Translation>();
    }
    public Language(string langName)
    {
        LangName = langName;
    }
}