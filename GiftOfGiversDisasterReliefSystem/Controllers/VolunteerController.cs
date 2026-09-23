using GiftOfGiversDisasterReliefSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfGiversDisasterReliefSystem.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VolunteerController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ==========================================
        // PUBLIC VOLUNTEER PAGE
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ==========================================
        // SUBMIT VOLUNTEER INTEREST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            string fullName,
            string email,
            string skills,
            string availability)
        {
            // Basic validation

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError(
                    "fullName",
                    "Please enter your full name.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "email",
                    "Please enter your email address.");
            }

            if (string.IsNullOrWhiteSpace(skills))
            {
                ModelState.AddModelError(
                    "skills",
                    "Please enter your skills.");
            }

            if (string.IsNullOrWhiteSpace(availability))
            {
                ModelState.AddModelError(
                    "availability",
                    "Please select your availability.");
            }


            if (!ModelState.IsValid)
            {
                return View();
            }


            // Create volunteer record

            var volunteer = new VolunteerInterest
            {
                FullName = fullName,
                Email = email,
                Skills = skills,
                Availability = availability,
                DateRegistered = DateTime.UtcNow
            };


            // Save to database

            _context.VolunteerInterests.Add(volunteer);

            await _context.SaveChangesAsync();


            // Show success message

            TempData["SuccessMessage"] =
                "Thank you! Your volunteer interest has been submitted successfully.";


            return RedirectToAction("Index");
        }
    }
}