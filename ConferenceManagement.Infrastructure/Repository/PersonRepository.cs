using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceManagement.Infrastructure.Repository;

public class PersonRepository
{
    private readonly Context _context;

    public Context UnitOfWork => _context;

    public PersonRepository(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Person> AddAsync(Person person)
    {
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        return person;
    }

    public async Task<List<Person>> GetAllAsync()
    {
        return await _context.Persons
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await _context.Persons
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Person?> GetByEmailAsync(string email)
    {
        return await _context.Persons
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task UpdateAsync(Person person)
    {
        _context.Persons.Update(person);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var person = await _context.Persons.FindAsync(id);

        if (person is null)
        {
            return;
        }

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();
    }

    public void ChangeTrackerClear()
    {
        _context.ChangeTracker.Clear();
    }
}