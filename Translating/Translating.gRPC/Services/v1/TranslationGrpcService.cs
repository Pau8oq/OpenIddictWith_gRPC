using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Translating.Domain.AggregatesModel.TranslationAggregate;
using Translating.gRPC.Mappers;
using TranslationService.v1;

namespace Translating.gRPC.Services.v1;

[Authorize]
public class TranslationGrpcService: TranslationService.v1.TranslationService.TranslationServiceBase
{
    private readonly ITranslationRepository _translationRepository;
    public TranslationGrpcService(ITranslationRepository translationRepository)
    {
        _translationRepository = translationRepository;
    }

    public override async Task Create(IAsyncStreamReader<TranslationCreationRequest> requestStream, IServerStreamWriter<TranslationCreationReply> responseStream, ServerCallContext context)
    {
        await foreach (var item in requestStream.ReadAllAsync())
        {
            var lang = new Domain.AggregatesModel.TranslationAggregate.Language(item.Language.LangName);

            var createdTranslation = await _translationRepository.AddAsync(new Translation(item.EnglishWord, item.TranslationText, lang));

            await responseStream.WriteAsync(new TranslationCreationReply()
            {
                Id = createdTranslation.Id,
                EnglishWord = createdTranslation.GetEnglishWord(),
                TranslationText = createdTranslation.GetTranslationText(),
                Language = new TranslationService.v1.Language() { LangName = createdTranslation.TranslationLanguage.GetLangName() }
            });
        }

        await Task.CompletedTask;
    }

    public override async Task GetAll(Empty request, IServerStreamWriter<TranslationReply> responseStream, ServerCallContext context)
    {
        var lst = await _translationRepository.GetAllAsync();

        foreach (var item in lst)
        {
            await responseStream.WriteAsync(item.ToReply());
        }

        await Task.CompletedTask;
    }

    public override async Task<Empty> Update(TranslationUpdateRequest request, ServerCallContext context)
    {
        //need to implemetn properly
        //var itempToUpdate = await _translationRepository.GetById(request.Id);

        //if (itempToUpdate != null)
        //{
        //    var lang = new Domain.AggregatesModel.TranslationAggregate.Language(request.Language.LangName); 

        //    var updateSucceed = await _translationRepository.UpdateAsync(new Translation(request.EnglishWord, request.TranslationText, lang) { Id  = request.Id })
        //}

        //var updateSucceed = await _translationRepository.UpdateAsync(new Translation() { Id })

        return new Empty();
    }
}