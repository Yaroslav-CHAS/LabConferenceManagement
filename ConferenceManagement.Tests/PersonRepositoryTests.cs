using ConferenceManagement.Domain.Model;
using Xunit;

namespace ConferenceManagement.Tests;

public class PersonRepositoryTests
{
    [Fact]
    public async Task AddAsync_AddsPerson()
    {
        using var helper = new TestHelper();

        var person = new Person
        {
            Id = Guid.NewGuid(),
            LastName = "Сидоров",
            FirstName = "Павел",
            MiddleName = "Алексеевич",
            Workplace = "Компания",
            Email = "sidorov@example.com",
            Phone = "+79992223344"
        };

        await helper.PersonRepository.AddAsync(person);

        helper.PersonRepository.ChangeTrackerClear();

        var result = await helper.PersonRepository.GetByIdAsync(person.Id);

        Assert.NotNull(result);
        Assert.Equal("Сидоров", result.LastName);
        Assert.Equal("sidorov@example.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsExistingPerson()
    {
        using var helper = new TestHelper();

        var result = await helper.PersonRepository
            .GetByEmailAsync("ivanov@example.com");

        Assert.NotNull(result);
        Assert.Equal("Иванов", result.LastName);
        Assert.Equal("Иван", result.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPerson()
    {
        using var helper = new TestHelper();

        var person = await helper.PersonRepository
            .GetByIdAsync(helper.Chairman.Id);

        Assert.NotNull(person);

        person.Phone = "+79995556677";
        await helper.PersonRepository.UpdateAsync(person);

        helper.PersonRepository.ChangeTrackerClear();

        var result = await helper.PersonRepository
            .GetByIdAsync(helper.Chairman.Id);

        Assert.NotNull(result);
        Assert.Equal("+79995556677", result.Phone);
    }

    [Fact]
    public async Task DeleteAsync_DeletesPerson()
    {
        using var helper = new TestHelper();

        var person = new Person
        {
            Id = Guid.NewGuid(),
            LastName = "Удаляемый",
            FirstName = "Тест",
            MiddleName = string.Empty,
            Workplace = string.Empty,
            Email = "delete@example.com",
            Phone = string.Empty
        };

        await helper.PersonRepository.AddAsync(person);
        await helper.PersonRepository.DeleteAsync(person.Id);

        helper.PersonRepository.ChangeTrackerClear();

        var result = await helper.PersonRepository.GetByIdAsync(person.Id);

        Assert.Null(result);
    }
}