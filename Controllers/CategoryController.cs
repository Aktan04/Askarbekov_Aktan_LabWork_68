using Hh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hh.Controllers;

public class CategoryController : Controller
{
    private HhContext _context;

    public CategoryController(HhContext context)
    {
        _context = context;
    }
    
    public IActionResult Index()
    {
        var categories = _context.Categories.ToList();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public IActionResult Create(Category category)
    {
        
        if (_context.Categories.Any(b => b.Name == category.Name))
        {
            ModelState.AddModelError("NameOfCategory", "Категория с таким названием уже существует!");
            return View(category);
        }

        if (ModelState.IsValid)
        {
            _context.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(category);
    }
}