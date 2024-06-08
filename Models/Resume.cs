namespace Hh.Models;

public class Resume
{
    
    public int Id { get; set; }
    public string Title { get; set; }
    public int ExpectedSalary { get; set; }
    public string Telegram { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    
    public string? FacebookLink { get; set; }
    public string? LinkedinLink { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsPublished { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public ICollection<WorkExperience>? WorkExperiences { get; set; }
    public ICollection<Education>? Educations { get; set; } 
}