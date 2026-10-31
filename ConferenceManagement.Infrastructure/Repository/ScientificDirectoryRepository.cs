using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceManagement.Infrastructure.Repository;

public class ScientificDirectionRepository
{
    private readonly Context _context;

    public Context UnitOfWork => _context;

    public ScientificDirectionRepository(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ScientificDirection> AddAsync(
        ScientificDirection direction)
    {
        _context.ScientificDirections.Add(direction);
        await _context.SaveChangesAsync();

        return direction;
    }

    public async Task<List<ScientificDirection>> GetAllAsync()
    {
        return await _context.ScientificDirections
            .Include(x => x.Sections)
            .OrderBy(x => x.Index)
            .ToListAsync();
    }

    public async Task<ScientificDirection?> GetByIdAsync(Guid id)
    {
        return await _context.ScientificDirections
            .Include(x => x.Sections)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ScientificDirection?> GetByIndexAsync(
        Guid conferenceId,
        string index)
    {
        return await _context.ScientificDirections
            .FirstOrDefaultAsync(x =>
                x.ConferenceId == conferenceId &&
                x.Index == index);
    }

    public async Task UpdateAsync(ScientificDirection direction)
    {
        var existingDirection = await _context.ScientificDirections
            .Include(x => x.Sections)
            .FirstOrDefaultAsync(x => x.Id == direction.Id);

        if (existingDirection is null)
        {
            return;
        }

        _context.Entry(existingDirection)
            .CurrentValues.SetValues(direction);

        foreach (var section in direction.Sections)
        {
            var existingSection = existingDirection.Sections
                .FirstOrDefault(x => x.Id == section.Id);

            if (existingSection is null)
            {
                section.ScientificDirectionId = existingDirection.Id;
                _context.Sections.Add(section);
            }
            else
            {
                _context.Entry(existingSection)
                    .CurrentValues.SetValues(section);
            }
        }

        foreach (var existingSection in existingDirection.Sections.ToList())
        {
            var sectionStillExists = direction.Sections
                .Any(x => x.Id == existingSection.Id);

            if (!sectionStillExists)
            {
                _context.Sections.Remove(existingSection);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var direction = await _context.ScientificDirections.FindAsync(id);

        if (direction is null)
        {
            return;
        }

        _context.ScientificDirections.Remove(direction);
        await _context.SaveChangesAsync();
    }

    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }
}