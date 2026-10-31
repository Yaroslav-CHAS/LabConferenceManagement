using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceManagement.Infrastructure.Repository;

public class ConferenceRepository
{
    private readonly Context _context;

    public Context UnitOfWork => _context;

    public ConferenceRepository(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Conference> AddAsync(Conference conference)
    {
        _context.Conferences.Add(conference);
        await _context.SaveChangesAsync();

        return conference;
    }

    public async Task<List<Conference>> GetAllAsync()
    {
        return await _context.Conferences
            .Include(x => x.Directions)
            .OrderBy(x => x.StartDate)
            .ToListAsync();
    }

    public async Task<Conference?> GetByIdAsync(Guid id)
    {
        return await _context.Conferences
            .Include(x => x.Directions)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Conference?> GetByNameAsync(string name)
    {
        return await _context.Conferences
            .Include(x => x.Directions)
            .FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task UpdateAsync(Conference conference)
    {
        var existingConference = await _context.Conferences
            .Include(x => x.Directions)
            .FirstOrDefaultAsync(x => x.Id == conference.Id);

        if (existingConference is null)
        {
            return;
        }

        _context.Entry(existingConference)
            .CurrentValues.SetValues(conference);

        foreach (var direction in conference.Directions)
        {
            var existingDirection = existingConference.Directions
                .FirstOrDefault(x => x.Id == direction.Id);

            if (existingDirection is null)
            {
                direction.ConferenceId = existingConference.Id;
                _context.ScientificDirections.Add(direction);
            }
            else
            {
                _context.Entry(existingDirection)
                    .CurrentValues.SetValues(direction);
            }
        }

        foreach (var existingDirection in existingConference.Directions.ToList())
        {
            var directionStillExists = conference.Directions
                .Any(x => x.Id == existingDirection.Id);

            if (!directionStillExists)
            {
                _context.ScientificDirections.Remove(existingDirection);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var conference = await _context.Conferences.FindAsync(id);

        if (conference is null)
        {
            return;
        }

        _context.Conferences.Remove(conference);
        await _context.SaveChangesAsync();
    }

    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }
}