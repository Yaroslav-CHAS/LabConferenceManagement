namespace ConferenceManagement.Infrastructure.Data;

public sealed class ConferenceAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsOrganizer { get; set; }
    public Guid? PersonId { get; set; }
}
