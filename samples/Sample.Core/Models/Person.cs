namespace Sample.Core.Models;

public class Person
{
    public Person() { }

    public Person(string firstName, string lastName, int score, string location)
    {
        FirstName = firstName;
        LastName = lastName;
        Score = score;
        Location = location;
    }

    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName => $"{FirstName} {LastName}";
    public int Score { get; set; }
    public string? Location { get; set; }
    public DateTime? Birthday { get; set; }

    public override string ToString()
        => FullName ?? string.Empty;
}
