using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;

namespace FougeraClub1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? SignatureImagePath { get; set; }
    }
}
