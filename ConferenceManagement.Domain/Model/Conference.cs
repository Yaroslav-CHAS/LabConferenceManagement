namespace ConferenceManagement.Domain.Model;

public class Conference
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public List<ScientificDirection> Directions { get; set; } = new();

    public void AddDirection(ScientificDirection direction)
    {
        Directions.Add(direction);
    }

    public void RemoveDirection(Guid directionId)
    {
        var direction = Directions.FirstOrDefault(x => x.Id == directionId);

        if (direction is not null)
        {
            Directions.Remove(direction);
        }
    }
}