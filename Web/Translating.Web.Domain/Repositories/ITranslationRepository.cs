using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translating.Web.Domain.Models;

namespace Translating.Web.Domain.Repositories
{
    public interface ITranslationRepository
    {
        Task<CreatedTranslationModel> AddAsync(CreateTranslationModel translation);
        Task<IEnumerable<TranslationModel>> GetAllAsync();
        Task UpdateAsync(TranslationModel translation);
        Task<TranslationModel> GetByIdAsync(int id);
    }
}
