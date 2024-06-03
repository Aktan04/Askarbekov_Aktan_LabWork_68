using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Hh.Models;

public class HhContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    
}