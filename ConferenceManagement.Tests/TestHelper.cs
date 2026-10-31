using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Data;
using ConferenceManagement.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ConferenceManagement.Tests;

public class TestHelper : IDisposable
{
    private readonly Context _context;

    public Person Chairman { get; }

    public Person Secretary { get; }

    public Conference Conference { get; }

    public ScientificDirection Direction { get; }

    public Section Section { get; }

    public PersonRepository PersonRepository => new(_context);

    public ScientificDirectionRepository DirectionRepository => new(_context);

    public SectionRepository SectionRepository => new(_context);

    public ConferenceRepository ConferenceRepository => new(_context);

    public TestHelper()
    {
        var databaseName = $"ConferenceManagementTests_{Guid.NewGuid():N}";

        var options = new DbContextOptionsBuilder<Context>()
            .UseSqlServer(
                $"Server=(localdb)\\mssqllocaldb;" +
                $"Database={databaseName};" +
                "Integrated Security=true;" +
                "TrustServerCertificate=True")
            .Options;

        _context = new Context(options);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        Chairman = new Person
        {
            Id = Guid.NewGuid(),
            LastName = "Иванов",
            FirstName = "Иван",
            MiddleName = "Иванович",
            Workplace = "Университет",
            Email = "ivanov@example.com",
            Phone = "+79990000000"
        };

        Secretary = new Person
        {
            Id = Guid.NewGuid(),
            LastName = "Петрова",
            FirstName = "Анна",
            MiddleName = "Сергеевна",
            Workplace = "Университет",
            Email = "petrova@example.com",
            Phone = "+79991111111"
        };

        Conference = new Conference
        {
            Id = Guid.NewGuid(),
            Name = "Научная конференция 2026",
            StartDate = new DateTime(2026, 11, 20),
            EndDate = new DateTime(2026, 11, 22)
        };

        Direction = new ScientificDirection
        {
            Id = Guid.NewGuid(),
            Name = "Информационные технологии",
            Index = "IT",
            Description = "Направление информационных технологий",
            ConferenceId = Conference.Id
        };

        Section = new Section
        {
            Id = Guid.NewGuid(),
            Name = "Веб-разработка",
            Index = "IT-1",
            Description = "Современные веб-технологии",
            Place = "Аудитория 301",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 12, 0, 0),
            ScientificDirectionId = Direction.Id,
            ChairmanId = Chairman.Id,
            SecretaryId = Secretary.Id
        };

        _context.AddRange(
            Chairman,
            Secretary,
            Conference,
            Direction,
            Section);

        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}