using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AccessRoles.Organizer)]
public class ScientificDirectionsController : ControllerBase
{
    private readonly ScientificDirectionRepository _repository;

    public ScientificDirectionsController(
        ScientificDirectionRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ScientificDirection>>> GetAll()
    {
        var directions = await _repository.GetAllAsync();

        return Ok(directions);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
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
        if (!IsValid(direction))
        {
            return BadRequest("Укажите название, индекс, место и время направления. Окончание должно быть позже начала.");
        }

        if (direction.Id == Guid.Empty)
        {
            direction.Id = Guid.NewGuid();
        }

        direction.Conference = null!;
        direction.Sections = [];
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
        if (!IsValid(direction))
        {
            return BadRequest("Укажите название, индекс, место и время направления. Окончание должно быть позже начала.");
        }

        var existingDirection = await _repository.GetByIdAsync(id);

        if (existingDirection is null)
        {
            return NotFound();
        }

        direction.Id = id;
        direction.Sections = existingDirection.Sections;

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

    private static bool IsValid(ScientificDirection direction) =>
        !string.IsNullOrWhiteSpace(direction.Name) &&
        !string.IsNullOrWhiteSpace(direction.Index) &&
        !string.IsNullOrWhiteSpace(direction.Place) &&
        direction.ConferenceId != Guid.Empty &&
        direction.StartTime.HasValue && direction.EndTime > direction.StartTime;
}
