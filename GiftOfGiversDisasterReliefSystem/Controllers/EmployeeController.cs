using GiftOfGiversDisasterReliefSystem.Data;
using GiftOfGiversDisasterReliefSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfGiversDisasterReliefSystem.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public EmployeeController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }


        // ==========================================
        // EMPLOYEE DASHBOARD
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }


            // ==========================================
            // COUNT VOLUNTEER REGISTRATIONS
            // ==========================================

            var volunteerCount =
                await _context.VolunteerInterests.CountAsync();


            // ==========================================
            // COUNT RELIEF PROJECT UPDATES
            // ==========================================

            var projectCount =
                await _context.ReliefUpdates.CountAsync();


            // ==========================================
            // COUNT DONATIONS
            // ==========================================

            var donationCount =
                await _context.Donations.CountAsync();


            // ==========================================
            // COUNT CONTACT MESSAGES
            // ==========================================

            var messageCount =
                await _context.ContactMessages.CountAsync();


            // ==========================================
            // SEND STATISTICS TO DASHBOARD
            // ==========================================

            ViewBag.VolunteerCount = volunteerCount;
            ViewBag.ProjectCount = projectCount;
            ViewBag.DonationCount = donationCount;
            ViewBag.MessageCount = messageCount;


            return View(user);
        }


        // ==========================================
        // RELIEF UPDATES - DISPLAY PAGE
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Updates()
        {
            var updates = await _context.ReliefUpdates
                .OrderByDescending(u => u.DatePosted)
                .ToListAsync();

            return View(updates);
        }


        // ==========================================
        // POST RELIEF UPDATE
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Updates(
            string title,
            string description)
        {
            // ==========================================
            // VALIDATE TITLE
            // ==========================================

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError(
                    "title",
                    "Please enter an update title.");
            }


            // ==========================================
            // VALIDATE DESCRIPTION
            // ==========================================

            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError(
                    "description",
                    "Please enter an update description.");
            }


            if (!ModelState.IsValid)
            {
                var existingUpdates =
                    await _context.ReliefUpdates
                        .OrderByDescending(u => u.DatePosted)
                        .ToListAsync();

                return View(existingUpdates);
            }


            // ==========================================
            // CREATE RELIEF UPDATE
            // ==========================================

            var reliefUpdate = new ReliefUpdate
            {
                Title = title.Trim(),
                Description = description.Trim(),

                // Store date consistently as UTC
                DatePosted = DateTime.UtcNow
            };


            // ==========================================
            // SAVE TO DATABASE
            // ==========================================

            _context.ReliefUpdates.Add(reliefUpdate);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Relief project update posted successfully.";


            return RedirectToAction("Updates");
        }


        // ==========================================
        // EMPLOYEE DONATIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Donations()
        {
            var employee = await _userManager.GetUserAsync(User);

            if (employee == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }


            // ==========================================
            // GET ALL DONATIONS
            // ==========================================

            var donations = await _context.Donations
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();


            // ==========================================
            // CREATE VIEW MODEL LIST
            // ==========================================

            var donationList =
                new List<EmployeeDonationViewModel>();


            // ==========================================
            // PROCESS EACH DONATION
            // ==========================================

            foreach (var donation in donations)
            {
                var donationViewModel =
                    new EmployeeDonationViewModel
                    {
                        Id = donation.Id,
                        Amount = donation.Amount,
                        DonationType = donation.DonationType,
                        Currency = donation.Currency,
                        IsAnonymous = donation.IsAnonymous,
                        DonationDate = donation.DonationDate,
                        TaxCertificateNumber =
                            donation.TaxCertificateNumber
                    };


                // ==========================================
                // ANONYMOUS DONATION
                // ==========================================

                if (donation.IsAnonymous)
                {
                    donationViewModel.DonorName =
                        "Anonymous Donor";

                    donationViewModel.DonorEmail =
                        "Hidden";
                }
                else
                {
                    // ==========================================
                    // FIND DONOR ACCOUNT
                    // ==========================================

                    var donor =
                        await _userManager.FindByIdAsync(
                            donation.UserId);


                    if (donor != null)
                    {
                        // ==========================================
                        // DONOR NAME
                        // ==========================================

                        if (!string.IsNullOrWhiteSpace(
                            donor.FullName))
                        {
                            donationViewModel.DonorName =
                                donor.FullName;
                        }
                        else
                        {
                            donationViewModel.DonorName =
                                donor.Email ?? "Donor";
                        }


                        // ==========================================
                        // DONOR EMAIL
                        // ==========================================

                        donationViewModel.DonorEmail =
                            donor.Email ?? "Not available";
                    }
                    else
                    {
                        donationViewModel.DonorName =
                            "Donor";

                        donationViewModel.DonorEmail =
                            "Not available";
                    }
                }


                donationList.Add(donationViewModel);
            }


            // ==========================================
            // SEND DONATIONS TO VIEW
            // ==========================================

            return View(donationList);
        }


        // ==========================================
        // CONTACT MESSAGES
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Messages()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.DateSent)
                .ToListAsync();

            return View(messages);
        }


        // ==========================================
        // MARK MESSAGE AS READ
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMessageAsRead(int id)
        {
            var message =
                await _context.ContactMessages
                    .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
            {
                return NotFound();
            }


            message.IsRead = true;

            await _context.SaveChangesAsync();


            return RedirectToAction("Messages");
        }


        // ==========================================
        // VOLUNTEER SIGN-UPS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Volunteers()
        {
            var volunteers =
                await _context.VolunteerInterests
                    .OrderByDescending(v => v.DateRegistered)
                    .ToListAsync();

            return View(volunteers);
        }


        // ==========================================
        // UPDATE VOLUNTEER STATUS
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVolunteerStatus(
            int id,
            string status)
        {
            var volunteer =
                await _context.VolunteerInterests
                    .FirstOrDefaultAsync(v => v.Id == id);

            if (volunteer == null)
            {
                return NotFound();
            }


            // ==========================================
            // VALID STATUSES
            // ==========================================

            if (status != "Waiting" &&
                status != "Approved" &&
                status != "Rejected")
            {
                return BadRequest();
            }


            // ==========================================
            // UPDATE STATUS
            // ==========================================

            volunteer.Status = status;

            await _context.SaveChangesAsync();


            return RedirectToAction("Volunteers");
        }


        // ==========================================
        // EMPLOYEE PROFILE - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }

            return View(user);
        }


        // ==========================================
        // EMPLOYEE PROFILE - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            string fullName,
            string email)
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }


            // ==========================================
            // VALIDATE FULL NAME
            // ==========================================

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError(
                    "fullName",
                    "Please enter your full name.");
            }


            // ==========================================
            // VALIDATE EMAIL
            // ==========================================

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "email",
                    "Please enter your email address.");
            }


            if (!ModelState.IsValid)
            {
                return View(user);
            }


            // ==========================================
            // UPDATE FULL NAME
            // ==========================================

            user.FullName = fullName.Trim();


            // ==========================================
            // UPDATE EMAIL
            // ==========================================

            if (!string.Equals(
                user.Email,
                email,
                StringComparison.OrdinalIgnoreCase))
            {
                var emailResult =
                    await _userManager.SetEmailAsync(
                        user,
                        email.Trim());

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            error.Description);
                    }

                    return View(user);
                }


                // ==========================================
                // KEEP USERNAME SAME AS EMAIL
                // ==========================================

                var usernameResult =
                    await _userManager.SetUserNameAsync(
                        user,
                        email.Trim());

                if (!usernameResult.Succeeded)
                {
                    foreach (var error in usernameResult.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            error.Description);
                    }

                    return View(user);
                }
            }


            // ==========================================
            // SAVE PROFILE
            // ==========================================

            var result =
                await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage =
                    "Your profile has been updated successfully.";

                await _signInManager.RefreshSignInAsync(user);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }
            }


            return View(user);
        }


        // ==========================================
        // EMPLOYEE LOGOUT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}