using Hh.Models;
using Hh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hh.Controllers;

public class VacationController : Controller
{
    private readonly HhContext _context;
    private readonly UserManager<User> _userManager;

    public VacationController(HhContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> Index()
    {
        var userId = Convert.ToInt32(_userManager.GetUserId(User));
        var vacations = await _context.Vacations
            .Where(v => v.UserId == userId).ToListAsync();

        return View(vacations);
    }
    
    [Authorize]
    public async Task<IActionResult> AllVacations(string? titleSearch, string? category, int page = 1, VacanciesSortState sortState = VacanciesSortState.DateAsc)
    {
        ViewBag.CategoryFromDb = await _context.Categories.Select(c => c.Name).ToListAsync();
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (!user.IsAbleToSeeVacations)
        {
            return NotFound();
        }
        IQueryable<Vacation> filteredVacations = _context.Vacations
            .Where(v => v.IsPublished == true)
            .Include(v => v.Category)
            .Include(v => v.User)
            .OrderBy(r => r.LastUpdated);
        if (!string.IsNullOrEmpty(ViewBag.Categories))
        {
            category = ViewBag.Categories;
        }
        if (!string.IsNullOrEmpty(ViewBag.TitleSearch))
        {
            titleSearch = ViewBag.TitleSearch;
        }
        var categoryForFilter = await _context.Categories.FirstOrDefaultAsync(c => c.Name == category);
        if (!string.IsNullOrEmpty(category))
            filteredVacations = filteredVacations.Where(t => t.CategoryId == categoryForFilter.Id);
        if (!string.IsNullOrEmpty(titleSearch))
            filteredVacations = filteredVacations.Where(t => t.Title == titleSearch);
        var vacations = await filteredVacations.ToListAsync();
        ViewBag.SalarySort = sortState == VacanciesSortState.SalaryAsc
            ? VacanciesSortState.SalaryDesc
            : VacanciesSortState.SalaryAsc;
        ViewBag.DateSort = sortState == VacanciesSortState.DateAsc
            ? VacanciesSortState.DateDesc
            : VacanciesSortState.DateAsc;
        if (sortState == VacanciesSortState.SalaryAsc)
            vacations = vacations.OrderBy(v => v.Salary).ToList();
        else if (sortState == VacanciesSortState.SalaryDesc)
            vacations = vacations.OrderByDescending(v => v.Salary).ToList();
        else if (sortState == VacanciesSortState.DateAsc)
            vacations = vacations.OrderBy(v => v.LastUpdated).ToList();
        else if (sortState == VacanciesSortState.DateDesc)
            vacations = vacations.OrderByDescending(v => v.LastUpdated).ToList();
        
        int pageSize = 20; 
        ViewBag.CurrentSort = sortState;
        ViewBag.CurrentFilter = vacations;
        ViewBag.TitleSearch = titleSearch;
        ViewBag.Categories = category;
        var count = vacations.Count;
        var items = vacations.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        
        var viewModel = new VacationIndexViewModel
        {
            Vacations = items,
            PageViewModel = new PageViewModel(count, page, pageSize)
        };
        return View(viewModel);
    }
    
    [HttpGet]
    [Authorize(Roles = "company")]
    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.ToList();
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Vacation vacation)
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        vacation.IsPublished = false;
        if (ModelState.IsValid)
        {
            vacation.UserId = Convert.ToInt32(_userManager.GetUserId(User));
            vacation.LastUpdated = DateTime.UtcNow.AddHours(6);
            _context.Vacations.Add(vacation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(vacation);
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var vacation = await _context.Vacations.FindAsync(id);
        if (vacation == null || vacation.UserId != userId)
        {
            return NotFound();
        }

        return View(vacation);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(Vacation vacation)
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        if (ModelState.IsValid)
        {
            var existingJob = await _context.Vacations.FindAsync(vacation.Id);
            if (existingJob == null || existingJob.UserId != userId)
            {
                return NotFound();
            }

            existingJob.Title = vacation.Title;
            existingJob.Salary = vacation.Salary;
            existingJob.Requirement = vacation.Requirement;
            existingJob.ExperienceFrom = vacation.ExperienceFrom;
            existingJob.ExperienceTo = vacation.ExperienceTo;
            existingJob.CategoryId = vacation.CategoryId;
            existingJob.LastUpdated = DateTime.UtcNow.AddHours(6);

            _context.Entry(existingJob).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(vacation);
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> UpdateDate(int id)
    {
        var userId = Convert.ToInt32(_userManager.GetUserId(User));
        var job = await _context.Vacations.FindAsync(id);
        if (job == null || job.UserId != userId)
        {
            return NotFound();
        }

        job.LastUpdated = DateTime.UtcNow.AddHours(6);
        _context.Entry(job).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    [Authorize(Roles = "company")]
    public async Task<IActionResult> PublishVacation(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var vacation = await _context.Vacations.FirstOrDefaultAsync(r => r.Id == id);
        if (vacation == null || vacation.UserId != userId)
        {
            return NotFound();
        }

        if (vacation.IsPublished)
        {
            vacation.IsPublished = false;
        }
        else
        {
            vacation.IsPublished = true;
        }
        _context.Update(vacation);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Details(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var vacation = await _context.Vacations
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id && v.IsPublished == true);

        if (vacation == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var userResumes = await _context.Resumes.Where(r => r.UserId == currentUser.Id && r.IsPublished == true).ToListAsync();

        var chats = await _context.Chats
            .Where(c => c.VacationId == id && c.Resume.UserId == currentUser.Id)
            .ToListAsync();
        var model = new VacationDetailViewModel
        {
            Vacation = vacation,
            UserResumes = userResumes,
            Chats = chats
        };

        return View(model);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Apply(int vacationId, int resumeId)
    {
        var userId = Convert.ToInt32(_userManager.GetUserId(User));
        var resume = _context.Resumes.FirstOrDefault(r => r.UserId == userId && r.Id == resumeId);
        if (resume == null || resume.UserId != userId)
        {
            return NotFound();
        }
    
        var chat = await _context.Chats
            .FirstOrDefaultAsync(r => r.VacationId == vacationId && r.ResumeId == resumeId);
        if (chat == null)
        {
            chat = new Chat
            {
                ResumeId = resumeId,
                VacationId = vacationId,
                Messages = new List<Message>()
            };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
        }

        return Json(new { redirectUrl = Url.Action("Chat", "Chat", new { chatId = chat.Id }) });
    }
    
    [Authorize(Roles = "company")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var vacation = await _context.Vacations.FindAsync(id);
        if (vacation != null && vacation.UserId == userId)
        {
            return NotFound();
        }

        _context.Vacations.Remove(vacation);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    
    

}