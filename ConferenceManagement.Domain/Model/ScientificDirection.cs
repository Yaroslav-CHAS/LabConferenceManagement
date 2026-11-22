namespace ConferenceManagement.Domain.Model;

public class ScientificDirection
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Index { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Place { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public Guid ConferenceId { get; set; }

    public Conference Conference { get; set; } = null!;

    public List<Section> Sections { get; set; } = new();

    public int SectionCount => Sections.Count;

    public void AddSection(Section section)
    {
        Sections.Add(section);
    }

    public void RemoveSection(Guid sectionId)
    {
        var section = Sections.FirstOrDefault(x => x.Id == sectionId);

        if (section is not null)
        {
            Sections.Remove(section);
        }
    }
}
