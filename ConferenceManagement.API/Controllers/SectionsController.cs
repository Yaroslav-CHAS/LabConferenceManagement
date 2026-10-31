using ConferenceManagement.Domain.Model;
using ConferenceManagement.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly SectionRepository _repository;

    public SectionsController(SectionRepository repository)
    {
        _repository = repository;
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
    public async Task<ActionResult<Section>> Create(Section section)
    {
        if (section.EndTime <= section.StartTime)
        {
            return BadRequest(
                "Время окончания должно быть позже времени начала.");
        }

        if (section.Id == Guid.Empty)
        {
            section.Id = Guid.NewGuid();
        }

        await _repository.AddAsync(section);

        return CreatedAtAction(
            nameof(GetById),
            new { id = section.Id },
            section);
    }

    [HttpPut("{id:guid}")]
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

        section.Id = id;

        await _repository.UpdateAsync(section);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
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
}