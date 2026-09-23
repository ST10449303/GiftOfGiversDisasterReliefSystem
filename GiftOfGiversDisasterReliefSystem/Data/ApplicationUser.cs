using Microsoft.AspNetCore.Identity;

namespace GiftOfGiversDisasterReliefSystem.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}