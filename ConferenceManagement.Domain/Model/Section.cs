namespace ConferenceManagement.Domain.Model;

public class Section
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Index { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Place { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public Guid ScientificDirectionId { get; set; }

    public ScientificDirection ScientificDirection { get; set; } = null!;

    public Guid ChairmanId { get; set; }

    public Person Chairman { get; set; } = null!;

    public Guid? SecretaryId { get; set; }

    public Person? Secretary { get; set; }

    public void Reschedule(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException(
                "Время окончания секции должно быть позже времени начала.");
        }

        StartTime = startTime;
        EndTime = endTime;
    }

    public void AssignChairman(Person person)
    {
        Chairman = person;
        ChairmanId = person.Id;
    }

    public void AssignSecretary(Person? person)
    {
        Secretary = person;
        SecretaryId = person?.Id;
    }
}