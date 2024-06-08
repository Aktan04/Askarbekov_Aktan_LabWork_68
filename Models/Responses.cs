namespace Hh.Models;

public class Responses
{
    public int Id { get; set; }
    public int VacationId { get; set; }
    public Vacation? Vacation { get; set; }
    public int ResumeId { get; set; }
    public Resume? Resume { get; set; }
    
}