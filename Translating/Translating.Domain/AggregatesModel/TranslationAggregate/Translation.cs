using Translating.Domain.SeedWork;

namespace Translating.Domain.AggregatesModel.TranslationAggregate;

public class Translation: Entity, IAggregateRoot
{
    private string EnglishWord;
    private string TranslationText;

    public string GetTranslationText() => TranslationText;
    public string GetEnglishWord() => EnglishWord;
    public Language TranslationLanguage {get;set;}

    public Translation()
    {

    }

    public Translation(string englishWord, string translation, Language tranlsationLang)
    {
        EnglishWord = englishWord;
        TranslationText = translation;
        TranslationLanguage = tranlsationLang;
    }
}