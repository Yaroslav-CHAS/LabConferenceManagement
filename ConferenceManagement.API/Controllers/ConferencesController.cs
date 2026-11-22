using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AccessRoles.Organizer)]
public class ConferencesController : ControllerBase
{
    private readonly ConferenceRepository _repository;

    public ConferencesController(ConferenceRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Conference>>> GetAll()
    {
        var conferences = await _repository.GetAllAsync();

        return Ok(conferences);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<Conference>> GetById(Guid id)
    {
        var conference = await _repository.GetByIdAsync(id);

        if (conference is null)
        {
            return NotFound();
        }

        return Ok(conference);
    }

    [HttpPost]
    public async Task<ActionResult<Conference>> Create(Conference conference)
    {
        if (string.IsNullOrWhiteSpace(conference.Name) || conference.StartDate == default)
        {
            return BadRequest("Укажите название и даты конференции.");
        }

        if (conference.EndDate < conference.StartDate)
        {
            return BadRequest(
                "Дата окончания конференции не может быть раньше даты начала.");
        }

        if (conference.Id == Guid.Empty)
        {
            conference.Id = Guid.NewGuid();
        }

        conference.Directions = [];
        await _repository.AddAsync(conference);

        return CreatedAtAction(
            nameof(GetById),
            new { id = conference.Id },
            conference);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Conference conference)
    {
        if (string.IsNullOrWhiteSpace(conference.Name) || conference.StartDate == default)
        {
            return BadRequest("Укажите название и даты конференции.");
        }

        if (conference.EndDate < conference.StartDate)
        {
            return BadRequest(
                "Дата окончания конференции не может быть раньше даты начала.");
        }

        var existingConference = await _repository.GetByIdAsync(id);

        if (existingConference is null)
        {
            return NotFound();
        }

        conference.Id = id;
        conference.Directions = existingConference.Directions;

        await _repository.UpdateAsync(conference);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var conference = await _repository.GetByIdAsync(id);

        if (conference is null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);

        return NoContent();
    }
}
