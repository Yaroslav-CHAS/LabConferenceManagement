using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly PersonRepository _repository;

    public PersonsController(PersonRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Person>>> GetAll()
    {
        var persons = await _repository.GetAllAsync();

        return Ok(persons);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Person>> GetById(Guid id)
    {
        var person = await _repository.GetByIdAsync(id);

        if (person is null)
        {
            return NotFound();
        }

        return Ok(person);
    }

    [HttpPost]
    public async Task<ActionResult<Person>> Create(Person person)
    {
        if (person.Id == Guid.Empty)
        {
            person.Id = Guid.NewGuid();
        }

        await _repository.AddAsync(person);

        return CreatedAtAction(
            nameof(GetById),
            new { id = person.Id },
            person);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Person person)
    {
        var existingPerson = await _repository.GetByIdAsync(id);

        if (existingPerson is null)
        {
            return NotFound();
        }

        person.Id = id;

        await _repository.UpdateAsync(person);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var person = await _repository.GetByIdAsync(id);

        if (person is null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);

        return NoContent();
    }
}