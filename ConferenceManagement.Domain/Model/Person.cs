namespace ConferenceManagement.Domain.Model;

public class Person
{
    public Guid Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string Workplace { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public List<Section> ChairedSections { get; set; } = new();

    public List<Section> SecretarySections { get; set; } = new();
}