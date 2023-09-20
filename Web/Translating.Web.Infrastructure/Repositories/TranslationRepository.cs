using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections;
using Translating.Web.Domain.Models;
using Translating.Web.Domain.Repositories;
using Translating.Web.Infrastructure.Mappers;
using Translating.Web.Infrastructure.v1;
using static Translating.Web.Infrastructure.v1.TranslationService;

namespace Translating.Web.Infrastructure.Repositories;

public class TranslationRepository : ITranslationRepository
{
    private readonly TranslationService.TranslationServiceClient _translationServiceClient;

    public TranslationRepository(TranslationService.TranslationServiceClient translationServiceClient)
    {
        _translationServiceClient = translationServiceClient;
    }

    public async Task<CreatedTranslationModel> AddAsync(CreateTranslationModel translation)
    {
        using var bidirectionnalStreamingCall = _translationServiceClient.Create();

        var translationToCreate = new TranslationCreationRequest
        {
            EnglishWord = translation.EnglishWord,
            TranslationText = translation.TranslationText,
            Language = new Language() { LangName = translation.Language.Name }
        };

        await bidirectionnalStreamingCall.RequestStream.WriteAsync(translationToCreate);

        await bidirectionnalStreamingCall.RequestStream.CompleteAsync();

        var res = new CreatedTranslationModel();

        await foreach (var item in bidirectionnalStreamingCall.ResponseStream.ReadAllAsync())
        {
            res = new CreatedTranslationModel
            {
                TranslationText = item.TranslationText,
                EnglishWord = item.EnglishWord,
                Language = new LanguageModel() { Id = item.Language.Id, Name = item.Language.LangName },
                Id = item.Id
            };
        }

        return res;
    }

    public async Task<IEnumerable<TranslationModel>> GetAllAsync()
    {
        //var metadata = new Metadata();
        //metadata.Add("authorization", $"bearer {token}");

        using var serverStreamCall = _translationServiceClient.GetAll(new Empty());

        var tr = new List<TranslationModel>();

        while (await serverStreamCall.ResponseStream.MoveNext())
        {
            tr.Add(serverStreamCall.ResponseStream.Current.ToDomain());
        }

        return tr;
    }

    public async Task<TranslationModel> GetByIdAsync(int id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(s => s.Id == id);
    }

    public async Task UpdateAsync(TranslationModel translation)
    {
        await _translationServiceClient.UpdateAsync(new TranslationUpdateRequest
        {
            Id = translation.Id,
            TranslationText = translation.TranslationText,
            EnglishWord = translation.EnglishWord,
            Language = new Language() { Id = translation.TranslationLanguage.Id, LangName = translation.TranslationLanguage.Name }
        });
    }
}