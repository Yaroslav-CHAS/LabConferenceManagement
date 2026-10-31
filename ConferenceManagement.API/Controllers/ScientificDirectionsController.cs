using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScientificDirectionsController : ControllerBase
{
    private readonly ScientificDirectionRepository _repository;

    public ScientificDirectionsController(
        ScientificDirectionRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<ScientificDirection>>> GetAll()
    {
        var directions = await _repository.GetAllAsync();

        return Ok(directions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ScientificDirection>> GetById(Guid id)
    {
        var direction = await _repository.GetByIdAsync(id);

        if (direction is null)
        {
            return NotFound();
        }

        return Ok(direction);
    }

    [HttpPost]
    public async Task<ActionResult<ScientificDirection>> Create(
        ScientificDirection direction)
    {
        if (direction.Id == Guid.Empty)
        {
            direction.Id = Guid.NewGuid();
        }

        await _repository.AddAsync(direction);

        return CreatedAtAction(
            nameof(GetById),
            new { id = direction.Id },
            direction);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        ScientificDirection direction)
    {
        var existingDirection = await _repository.GetByIdAsync(id);

        if (existingDirection is null)
        {
            return NotFound();
        }

        direction.Id = id;

        await _repository.UpdateAsync(direction);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var direction = await _repository.GetByIdAsync(id);

        if (direction is null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);

        return NoContent();
    }
}