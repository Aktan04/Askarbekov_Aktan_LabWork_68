using System.Security.Claims;
using Hh.Models;
using Hh.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hh.Controllers;

public class ResumeController : Controller
{
    private HhContext _context;
    private readonly UserManager<User> _userManager;
    
    public ResumeController(HhContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Index()
    {
        int? userId = Convert.ToInt32(_userManager.GetUserId(User));
        var resumes = await _context.Resumes
            .Where(r => r.UserId == userId)
            .Include(r => r.Category)
            .ToListAsync();
        return View(resumes);
    }
    
    [Authorize]
    public async Task<IActionResult> AllResumes(string? category)
    {
        ViewBag.CategoryFromDb = await _context.Categories.Select(c => c.Name).ToListAsync();
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var resumes = await _context.Resumes
            .Where(r => r.IsPublished == true)
            .Include(r => r.Category)
            .Include(r => r.User)
            .OrderByDescending(r => r.LastUpdated)
            .ToListAsync();
        var categoryForFilter = await _context.Categories.FirstOrDefaultAsync(c => c.Name == category);
        if (!string.IsNullOrEmpty(category))
            resumes = resumes.Where(t => t.CategoryId == categoryForFilter.Id).ToList();
        var resumesOfTargetUser = await _context.Vacations.Where(v => v.UserId == userId).OrderBy(r => r.LastUpdated).ToListAsync();
        ViewBag.Vacations = resumesOfTargetUser;
        return View(resumes);
    }
    
    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var resume = await _context.Resumes
            .Include(r => r.Category)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resume == null)
        {
            return NotFound();
        }

        return View(resume);
    }
    
    [HttpGet]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View();
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] ResumeData resumeData)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        if (ModelState.IsValid)
        {
            var resume = resumeData.Resume;
            resume.UserId = userId;
            resume.LastUpdated = DateTime.UtcNow.AddHours(6);
            resume.IsPublished = false;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            if (resumeData.WorkExperiences != null)
            {
                foreach (var workExperience in resumeData.WorkExperiences)
                {
                    workExperience.StartDate = workExperience.StartDate.ToUniversalTime().AddHours(6);
                    workExperience.EndDate = workExperience.EndDate?.ToUniversalTime().AddHours(6);
                    workExperience.ResumeId = resume.Id;
                    _context.WorkExperiences.Add(workExperience);
                }
            }

            if (resumeData.Educations != null)
            {
                foreach (var education in resumeData.Educations)
                {
                    education.StartDate = education.StartDate.ToUniversalTime().AddHours(6);
                    education.EndDate = education.EndDate?.ToUniversalTime().AddHours(6);
                    education.ResumeId = resume.Id;
                    _context.Educations.Add(education);
                }
            }

            user.IsAbleToSeeVacations = true; 
            _context.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        return Json(new { success = false, error = "Invalid data" });
    }
    
    [HttpGet]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Edit(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        ViewBag.Categories = await _context.Categories.ToListAsync();
        var resume = await _context.Resumes
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resume == null || resume.UserId != userId)
        {
            return NotFound();
        }

        var resumeData = new ResumeData
        {
            Resume = resume,
            WorkExperiences = resume.WorkExperiences.ToList(),
            Educations = resume.Educations.ToList()
        };

        return View(resumeData);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit([FromBody] ResumeData resumeData)
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        if (ModelState.IsValid)
        {
            var resume = await _context.Resumes.FindAsync(resumeData.Resume.Id);
            if (resume == null)
            {
                return NotFound();
            }

            resume.Title = resumeData.Resume.Title;
            resume.CategoryId = resumeData.Resume.CategoryId;
            resume.ExpectedSalary = resumeData.Resume.ExpectedSalary;
            resume.Telegram = resumeData.Resume.Telegram;
            resume.Email = resumeData.Resume.Email;
            resume.PhoneNumber = resumeData.Resume.PhoneNumber;
            resume.FacebookLink = resumeData.Resume.FacebookLink;
            resume.LinkedinLink = resumeData.Resume.LinkedinLink;
            resume.LastUpdated = DateTime.UtcNow.AddHours(6);

            _context.Entry(resume).State = EntityState.Modified;

            foreach (var workExperience in resumeData.WorkExperiences)
            {
                if (workExperience.Id == 0)
                {
                    workExperience.StartDate = workExperience.StartDate.ToUniversalTime().AddHours(6);
                    workExperience.EndDate = workExperience.EndDate?.ToUniversalTime().AddHours(6);
                    workExperience.ResumeId = resume.Id;
                    _context.WorkExperiences.Add(workExperience);
                }
                else
                {
                    var existingWorkExperience = await _context.WorkExperiences.FindAsync(workExperience.Id);
                    if (existingWorkExperience != null)
                    {
                        existingWorkExperience.CompanyName = workExperience.CompanyName;
                        existingWorkExperience.Position = workExperience.Position;
                        existingWorkExperience.Responsibilities = workExperience.Responsibilities;
                        existingWorkExperience.StartDate = workExperience.StartDate.ToUniversalTime().AddHours(6);
                        existingWorkExperience.EndDate = workExperience.EndDate?.ToUniversalTime().AddHours(6);

                        _context.Entry(existingWorkExperience).State = EntityState.Modified;
                    }
                }
            }

            foreach (var education in resumeData.Educations)
            {
                if (education.Id == 0)
                {
                    education.StartDate = education.StartDate.ToUniversalTime().AddHours(6);
                    education.EndDate = education.EndDate?.ToUniversalTime().AddHours(6);
                    education.ResumeId = resume.Id;
                    _context.Educations.Add(education);
                }
                else
                {
                    var existingEducation = await _context.Educations.FindAsync(education.Id);
                    if (existingEducation != null)
                    {
                        existingEducation.Institution = education.Institution;
                        existingEducation.Degree = education.Degree;
                        existingEducation.StartDate = education.StartDate.ToUniversalTime().AddHours(6);
                        existingEducation.EndDate = education.EndDate?.ToUniversalTime().AddHours(6);

                        _context.Entry(existingEducation).State = EntityState.Modified;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        return Json(new { success = false, error = "Invalid data" });
    }
    
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Update(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));

        var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id);
        if (resume == null || resume.UserId != userId)
        {
            return NotFound();
        }

        resume.LastUpdated = DateTime.UtcNow.AddHours(6);
        _context.Update(resume);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "user")]
    public async Task<IActionResult> PublishResume(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));

        var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id);
        if (resume == null || resume.UserId != userId)
        {
            return NotFound();
        }

        if (resume.IsPublished)
        {
            resume.IsPublished = false;
        }
        else
        {
            resume.IsPublished = true;
        }
        _context.Update(resume);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var resume = await _context.Resumes.FindAsync(id);
        if (resume == null || resume.UserId != userId)
        {
            return NotFound();
        }

        _context.Resumes.Remove(resume);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    
    [Authorize(Roles = "user")]
    public async Task<IActionResult> DeleteWorkExperience(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.UserId == userId);
        var workExperience = await _context.WorkExperiences.FindAsync(id);
        if (workExperience != null && workExperience.ResumeId == resume.Id)
        {
            return NotFound();
        }

        _context.WorkExperiences.Remove(workExperience);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id = workExperience.ResumeId });
    }

    [Authorize(Roles = "user")]
    public async Task<IActionResult> DeleteEducation(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.UserId == userId);
        var education = await _context.Educations.FindAsync(id);
        if (education != null && education.ResumeId == resume.Id)
        {
            return NotFound();
        }

        _context.Educations.Remove(education);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id = education.ResumeId });
    }
    
    
}