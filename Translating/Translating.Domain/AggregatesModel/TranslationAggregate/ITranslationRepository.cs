using System.Threading.Tasks;

namespace Translating.Domain.AggregatesModel.TranslationAggregate;

public interface ITranslationRepository
{
    Task<Translation> AddAsync(Translation translation);
    Task<IEnumerable<Translation>> GetAllAsync();
    Task UpdateAsync(Translation translation);
    Task<Translation> GetByIdAsync(int id);
}