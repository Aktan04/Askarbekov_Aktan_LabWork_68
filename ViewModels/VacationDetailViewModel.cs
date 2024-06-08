using Hh.Models;

namespace Hh.ViewModels;

public class VacationDetailViewModel
{
    public Vacation Vacation { get; set; }
    public List<Resume> UserResumes { get; set; }
    public List<Chat> Chats { get; set; }
    
}