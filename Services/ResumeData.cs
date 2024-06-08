using Hh.Models;

namespace Hh.Services;

public class ResumeData
{
    public Resume Resume { get; set; }
    public List<WorkExperience> WorkExperiences { get; set; }
    public List<Education> Educations { get; set; }
}