using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConferenceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly SectionRepository _repository;
    private readonly AccountRepository _accounts;

    public SectionsController(SectionRepository repository, AccountRepository accounts)
    {
        _repository = repository;
        _accounts = accounts;
    }

    [HttpGet]
    public async Task<ActionResult<List<Section>>> GetAll()
    {
        var sections = await _repository.GetAllAsync();

        return Ok(sections);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Section>> GetById(Guid id)
    {
        var section = await _repository.GetByIdAsync(id);

        if (section is null)
        {
            return NotFound();
        }

        return Ok(section);
    }

    [HttpGet("direction/{scientificDirectionId:guid}")]
    public async Task<ActionResult<List<Section>>> GetByDirectionId(
        Guid scientificDirectionId)
    {
        var sections = await _repository
            .GetByDirectionIdAsync(scientificDirectionId);

        return Ok(sections);
    }

    [HttpPost]
    [Authorize(Roles = AccessRoles.Organizer)]
    public async Task<ActionResult<Section>> Create(Section section)
    {
        if (!HasDetails(section))
        {
            return BadRequest("Укажите название, индекс, место, направление и председателя.");
        }

        if (section.EndTime <= section.StartTime)
        {
            return BadRequest(
                "Время окончания должно быть позже времени начала.");
        }

        if (section.Id == Guid.Empty)
        {
            section.Id = Guid.NewGuid();
        }

        section.ScientificDirection = null!;
        section.Chairman = null!;
        section.Secretary = null;
        await _repository.AddAsync(section);

        return CreatedAtAction(
            nameof(GetById),
            new { id = section.Id },
            section);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AccessRoles.Organizer + "," + AccessRoles.Responsible)]
    public async Task<IActionResult> Update(Guid id, Section section)
    {
        if (section.EndTime <= section.StartTime)
        {
            return BadRequest(
                "Время окончания должно быть позже времени начала.");
        }

        var existingSection = await _repository.GetByIdAsync(id);

        if (existingSection is null)
        {
            return NotFound();
        }

        if (User.IsInRole(AccessRoles.Organizer))
        {
            if (!HasDetails(section))
            {
                return BadRequest("Укажите название, индекс, место, направление и председателя.");
            }

            section.Id = id;
            await _repository.UpdateAsync(section);
        }
        else
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var personId = Guid.TryParse(accountId, out var accountKey)
                ? (await _accounts.GetAsync(accountKey))?.PersonId
                : null;
            if (personId is null ||
                (personId != existingSection.ChairmanId && personId != existingSection.SecretaryId))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(section.Place))
            {
                return BadRequest("Укажите место проведения.");
            }

            existingSection.Description = section.Description;
            existingSection.Place = section.Place;
            existingSection.Reschedule(section.StartTime, section.EndTime);
            await _repository.UpdateAsync(existingSection);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AccessRoles.Organizer)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var section = await _repository.GetByIdAsync(id);

        if (section is null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);

        return NoContent();
    }

    private static bool HasDetails(Section section) =>
        !string.IsNullOrWhiteSpace(section.Name) &&
        !string.IsNullOrWhiteSpace(section.Index) &&
        !string.IsNullOrWhiteSpace(section.Place) &&
        section.ScientificDirectionId != Guid.Empty &&
        section.ChairmanId != Guid.Empty;
}
