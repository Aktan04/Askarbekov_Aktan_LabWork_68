using Hh.Models;

namespace Hh.ViewModels;

public class VacationIndexViewModel
{
    public IEnumerable<Vacation> Vacations { get; set; }
    public PageViewModel PageViewModel { get; set; }
    public string? CurrentFilter { get; set; }
    public string? CurrentCategory { get; set; }
    public VacanciesSortState CurrentSort { get; set; }
}