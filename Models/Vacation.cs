namespace Hh.Models;

public class Vacation
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Salary { get; set; }
    public string Requirement { get; set; }
    public int ExperienceFrom { get; set; }
    public int ExperienceTo { get; set; }    
    public DateTime LastUpdated { get; set; }
    public bool IsPublished { get; set; }
    
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}