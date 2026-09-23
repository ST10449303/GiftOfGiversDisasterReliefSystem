using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftOfGiversDisasterReliefSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<VolunteerInterest> VolunteerInterests
        {
            get;
            set;
        }

        public DbSet<ReliefUpdate> ReliefUpdates
        {
            get;
            set;
        }

        public DbSet<Donation> Donations
        {
            get;
            set;
        }

        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Donation>()
                .Property(d => d.Amount)
                .HasPrecision(18, 2);
        }
    }
}