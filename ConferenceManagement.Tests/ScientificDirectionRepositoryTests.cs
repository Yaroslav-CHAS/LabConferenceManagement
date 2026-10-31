using ConferenceManagement.Domain.Model;
using Xunit;

namespace ConferenceManagement.Tests;

public class ScientificDirectionRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsDirectionWithSections()
    {
        using var helper = new TestHelper();

        var result = await helper.DirectionRepository
            .GetByIdAsync(helper.Direction.Id);

        Assert.NotNull(result);
        Assert.Equal("IT", result.Index);
        Assert.Single(result.Sections);
        Assert.Equal("Веб-разработка", result.Sections[0].Name);
    }

    [Fact]
    public async Task AddAsync_AddsDirection()
    {
        using var helper = new TestHelper();

        var direction = new ScientificDirection
        {
            Id = Guid.NewGuid(),
            Name = "Искусственный интеллект",
            Index = "AI",
            Description = "Направление искусственного интеллекта",
            ConferenceId = helper.Conference.Id
        };

        await helper.DirectionRepository.AddAsync(direction);

        helper.DirectionRepository.ChangeTrackerClear();

        var result = await helper.DirectionRepository
            .GetByIdAsync(direction.Id);

        Assert.NotNull(result);
        Assert.Equal("AI", result.Index);
    }

    [Fact]
    public async Task UpdateAsync_AddsSectionToDirection()
    {
        using var helper = new TestHelper();

        var direction = await helper.DirectionRepository
            .GetByIdAsync(helper.Direction.Id);

        Assert.NotNull(direction);

        helper.DirectionRepository.ChangeTrackerClear();

        direction.Sections.Add(new Section
        {
            Id = Guid.NewGuid(),
            Name = "Облачные технологии",
            Index = "IT-2",
            Description = "Секция облачных технологий",
            Place = "Аудитория 302",
            StartTime = new DateTime(2026, 11, 20, 13, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 15, 0, 0),
            ScientificDirectionId = direction.Id,
            ChairmanId = helper.Chairman.Id,
            SecretaryId = helper.Secretary.Id
        });

        await helper.DirectionRepository.UpdateAsync(direction);

        helper.DirectionRepository.ChangeTrackerClear();

        var result = await helper.DirectionRepository
            .GetByIdAsync(direction.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result.Sections.Count);
    }

    [Fact]
    public async Task UpdateAsync_RemovesSectionFromDirection()
    {
        using var helper = new TestHelper();

        var direction = await helper.DirectionRepository
            .GetByIdAsync(helper.Direction.Id);

        Assert.NotNull(direction);

        helper.DirectionRepository.ChangeTrackerClear();

        direction.Sections.Clear();

        await helper.DirectionRepository.UpdateAsync(direction);

        helper.DirectionRepository.ChangeTrackerClear();

        var result = await helper.DirectionRepository
            .GetByIdAsync(direction.Id);

        Assert.NotNull(result);
        Assert.Empty(result.Sections);
    }
}