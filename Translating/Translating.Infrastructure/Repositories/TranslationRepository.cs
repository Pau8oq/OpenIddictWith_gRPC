using Microsoft.EntityFrameworkCore;
using Translating.Domain.AggregatesModel.TranslationAggregate;

namespace Translating.Infrastructure.Repositories;

public class TranslationRepository: ITranslationRepository
{
    private readonly TranslationContext _context;

    public TranslationRepository(TranslationContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Translation> AddAsync(Translation translation)
    {
        var t = _context.Translations.Add(translation).Entity;
        await _context.SaveChangesAsync();
        return t;
    }

    public async Task<IEnumerable<Translation>> GetAllAsync()
    {
        return await _context.Translations
        .Include(w=>w.TranslationLanguage)
        .ToListAsync();
    }

    public async Task<Translation> GetByIdAsync(int id)
    {
        return await _context.Translations.FindAsync(id);
    }

    public async Task UpdateAsync(Translation translation)
    {
        _context.Entry(translation).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}