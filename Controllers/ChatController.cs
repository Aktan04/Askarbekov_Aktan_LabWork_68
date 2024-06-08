using Hh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hh.Controllers;

public class ChatController : Controller
{
    private readonly HhContext _context;
    private readonly UserManager<User> _userManager;

    public ChatController(HhContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "company")]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var chats = await _context.Chats
            .Include(r => r.Vacation)
            .Include(r => r.Resume)
            .Where(r => r.Vacation.UserId == currentUser.Id)
            .ToListAsync();

        return View(chats);
    }

    [Authorize(Roles = "user, company")]
    public async Task<IActionResult> Chat(int chatId)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        ViewBag.TargetUserId = userId;
        var response = await _context.Chats
            .Include(r => r.Resume)
            .Include(r => r.Vacation)
            .FirstOrDefaultAsync(r => r.Id == chatId);

        if (response == null)
        {
            return NotFound();
        }

        var chat = await _context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.ResumeId == response.ResumeId && c.VacationId == response.VacationId);

        if (chat == null)
        {
            chat = new Chat
            {
                ResumeId = response.ResumeId,
                VacationId = response.VacationId,
                Messages = new List<Message>()
            };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
        }

        return View(chat);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendMessage(int chatId, string messageText)
    {
        var chat = await _context.Chats.Include(c => c.Messages).FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);

        var message = new Message
        {
            ChatId = chat.Id,
            Text = messageText,
            UserId = currentUser.Id,
            DateOfSend = DateTime.UtcNow.AddHours(6)
        };

        chat.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Roles = "user")]
    public async Task<IActionResult> MyResponses()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var chats = await _context.Chats
            .Include(r => r.Vacation)
            .Where(r => r.Resume.UserId == currentUser.Id)
            .ToListAsync();

        return View(chats);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetChatMessages(int chatId)
    {
        var chat = await _context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat == null)
        {
            return NotFound();
        }

        var messages = chat.Messages.Select(m => new
        {
            text = m.Text,
            dateOfSend = m.DateOfSend.ToString("dd.MM.yyyy HH:mm"),
            userId = m.UserId
        }).ToList();
        
        return Json(new { messages });
    }
    
    [Authorize(Roles = "company")]
    public async Task<IActionResult> OpenOrCreateChat(int resumeId, int vacationId)
    {
        var userId = Convert.ToInt32(_userManager.GetUserId(User));
        var resume = await _context.Resumes.FindAsync(resumeId);

        if (resume == null)
        {
            return NotFound();
        }

        var chat = await _context.Chats
            .FirstOrDefaultAsync(c => c.ResumeId == resumeId && c.VacationId == vacationId);

        if (chat == null)
        {
            var vacation = await _context.Vacations.FindAsync(vacationId);
            if (vacation == null || vacation.UserId != userId)
            {
                return NotFound();
            }

            chat = new Chat
            {
                ResumeId = resumeId,
                VacationId = vacation.Id,
                Messages = new List<Message>()
            };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Chat", new { chatId = chat.Id });
    }
}

