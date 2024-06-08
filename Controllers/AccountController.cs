using Hh.Models;
using Hh.Services;
using Hh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Hh.Controllers;

public class AccountController : Controller
{
    public HhContext _context;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _hostEnvironment;
    
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment hostEnvironment, HhContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _hostEnvironment = hostEnvironment;
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        return View(user);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> EditProfile(User user)
    {
        User? findUserWithEmail = _context.Users.FirstOrDefault(u => u.Email == user.Email && u.Id == user.Id);
        User? findUserWithUserName = _context.Users.FirstOrDefault(u => u.UserName == user.UserName && u.Id == user.Id);
        ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
        List<string> errors = new List<string>();
        if (findUserWithEmail != null || findUserWithUserName != null)
        {
            ModelState.AddModelError(string.Empty, "Логин или Email уже существует");
            errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors });
        }
        string? userId = _userManager.GetUserId(User);
        User identityUser = await _userManager.FindByIdAsync(userId);
        if (identityUser != null)
        {
            if (ModelState.IsValid)
            {
                if (user.ImageFile != null && user.ImageFile.Length > 0)
                {
                    var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(user.ImageFile.FileName);
                    var fullPath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await user.ImageFile.CopyToAsync(fileStream);
                    }

                    identityUser.Avatar = "/images/" + fileName;
                }

                identityUser.UserName = user.UserName;
                identityUser.NickName = user.NickName;
                identityUser.Email = user.Email;
                var result = await _userManager.UpdateAsync(identityUser);
                if (result.Succeeded)
                {
                    return Json(new { success = true, user = new { id = identityUser.Id, userName = identityUser.UserName, nickName = identityUser.NickName, email = identityUser.Email, avatar = identityUser.Avatar } });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        } 
        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return Json(new { success = false, errors });
    }
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
       return View(new LoginViewModel(){ReturnUrl = returnUrl});
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(model.Email);
            }
            if (user != null)
            {
                
                SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");// change redirect
                }
            }
            ModelState.AddModelError("", "Invalid email or password");
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        User? findUserWithEmail = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        User? findUserWithUserName = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
        ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
        if (findUserWithEmail != null || findUserWithUserName != null)
        {
            ModelState.AddModelError("UserName", "Логин или Email уже существует");
            return View(model);
        }
        model.Avatar = "/images/default.png";
        if (ModelState.IsValid)
        {
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                
                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var fullPath = Path.Combine(uploadPath, fileName);
            
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
            
                model.Avatar = "/images/" + fileName;
            }
            
            User user = new User()
            {
                Email = model.Email,
                UserName = model.UserName,
                Avatar = model.Avatar,
                NickName = model.NickName,
                PhoneNumber = model.PhoneNumber,
                Role = model.Role
            };
            if (model.Role == UserRole.Company)
            {
                user.IsAbleToSeeVacations = true;
            }
            else
            {
                user.IsAbleToSeeVacations = false;
            }
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                string roleName = model.Role.ToString().ToLower();
                await _userManager.AddToRoleAsync(user, roleName);
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");// change redirect 
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> AccessDenied(string returnUrl = null)
    {
        return RedirectToAction("Login", new { returnUrl = returnUrl });
    }
}