using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hh.Services;
using Microsoft.AspNetCore.Identity;

namespace Hh.Models;

public class User : IdentityUser<int>
{
    public string? Avatar { get; set; }
    [Required(ErrorMessage = "Имя пользователя обязателен к заполнению")]
    public string NickName { get; set; }
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public UserRole? Role { get; set; }
    public bool IsAbleToSeeVacations { get; set; }
}