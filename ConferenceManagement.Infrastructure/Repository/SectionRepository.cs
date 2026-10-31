using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceManagement.Infrastructure.Repository;

public class SectionRepository
{
    private readonly Context _context;

    public Context UnitOfWork => _context;

    public SectionRepository(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Section> AddAsync(Section section)
    {
        ValidateTime(section);

        _context.Sections.Add(section);
        await _context.SaveChangesAsync();

        return section;
    }

    public async Task<List<Section>> GetAllAsync()
    {
        return await _context.Sections
            .Include(x => x.ScientificDirection)
            .Include(x => x.Chairman)
            .Include(x => x.Secretary)
            .OrderBy(x => x.StartTime)
            .ToListAsync();
    }

    public async Task<Section?> GetByIdAsync(Guid id)
    {
        return await _context.Sections
            .Include(x => x.ScientificDirection)
            .Include(x => x.Chairman)
            .Include(x => x.Secretary)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Section>> GetByDirectionIdAsync(
        Guid scientificDirectionId)
    {
        return await _context.Sections
            .Include(x => x.Chairman)
            .Include(x => x.Secretary)
            .Where(x => x.ScientificDirectionId == scientificDirectionId)
            .OrderBy(x => x.StartTime)
            .ToListAsync();
    }

    public async Task UpdateAsync(Section section)
    {
        ValidateTime(section);

        var existingSection = await _context.Sections
            .FirstOrDefaultAsync(x => x.Id == section.Id);

        if (existingSection is null)
        {
            return;
        }

        _context.Entry(existingSection)
            .CurrentValues.SetValues(section);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var section = await _context.Sections.FindAsync(id);

        if (section is null)
        {
            return;
        }

        _context.Sections.Remove(section);
        await _context.SaveChangesAsync();
    }

    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }

    private static void ValidateTime(Section section)
    {
        if (section.EndTime <= section.StartTime)
        {
            throw new ArgumentException(
                "Время окончания секции должно быть позже времени начала.");
        }
    }
}