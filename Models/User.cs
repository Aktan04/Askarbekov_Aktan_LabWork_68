using Microsoft.AspNetCore.Identity;

namespace Hh.Models;

public class User : IdentityUser<int>
{
    public string NickName { get; set; }
}