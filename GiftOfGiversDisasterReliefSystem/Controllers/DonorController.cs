using GiftOfGiversDisasterReliefSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GiftOfGiversDisasterReliefSystem.Controllers
{
    [Authorize(Roles = "Donor")]
    public class DonorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public DonorController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }


        // ==========================================
        // DONOR DASHBOARD
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

            var donations = await _context.Donations
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            ViewBag.DonationCount = donations.Count;

            ViewBag.TotalDonations =
                donations.Sum(d => d.Amount);

            ViewBag.CertificateCount =
                donations.Count(d =>
                    !string.IsNullOrWhiteSpace(
                        d.TaxCertificateNumber));

            var lastDonation =
                donations.FirstOrDefault();

            ViewBag.LastDonation =
                lastDonation != null
                    ? $"{lastDonation.Currency} {lastDonation.Amount:N2}"
                    : "No donations yet";

            return View(user);
        }


        // ==========================================
        // DONATION PAGE - GET
        // ==========================================

        [HttpGet]
        public IActionResult Donate()
        {
            return View();
        }


        // ==========================================
        // PROCESS DONOR DONATION - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(
            decimal amount,
            string donationType,
            string currency)
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
            // VALIDATE AMOUNT
            // ==========================================

            if (amount <= 0)
            {
                ModelState.AddModelError(
                    "amount",
                    "Please enter a valid donation amount.");
            }


            // ==========================================
            // VALIDATE DONATION TYPE
            // ==========================================

            if (donationType != "One-Time" &&
                donationType != "Recurring")
            {
                ModelState.AddModelError(
                    "donationType",
                    "Please select a valid donation type.");
            }


            // ==========================================
            // VALIDATE CURRENCY
            // ==========================================

            if (currency != "ZAR" &&
                currency != "USD" &&
                currency != "EUR")
            {
                ModelState.AddModelError(
                    "currency",
                    "Please select a valid currency.");
            }


            if (!ModelState.IsValid)
            {
                return View();
            }


            // ==========================================
            // GENERATE TAX CERTIFICATE NUMBER
            // ==========================================

            var certificateNumber =
                $"GOG-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
                .Substring(0, 28);


            // ==========================================
            // CREATE DONATION
            // ==========================================

            var donation = new Donation
            {
                UserId = user.Id,
                Amount = amount,
                DonationType = donationType,
                Currency = currency,
                IsAnonymous = false,
                DonationDate = DateTime.UtcNow,
                TaxCertificateNumber =
                    certificateNumber
            };


            // ==========================================
            // SAVE DONATION
            // ==========================================

            _context.Donations.Add(donation);

            await _context.SaveChangesAsync();


            // ==========================================
            // CONFIRMATION DETAILS
            // ==========================================

            ViewBag.Amount =
                donation.Amount;

            ViewBag.DonationType =
                donation.DonationType;

            ViewBag.Currency =
                donation.Currency;

            ViewBag.CertificateNumber =
                donation.TaxCertificateNumber;

            ViewBag.DonationDate =
                donation.DonationDate;


            return View(
                "DonationConfirmation",
                donation);
        }


        // ==========================================
        // DONATION HISTORY
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> DonationHistory()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }


            var donations =
                await _context.Donations
                    .Where(d => d.UserId == user.Id)
                    .OrderByDescending(d => d.DonationDate)
                    .ToListAsync();


            return View(donations);
        }


        // ==========================================
        // DOWNLOAD TAX CERTIFICATE AS PDF
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> DownloadTaxCertificate(
            int id)
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
            // FIND DONATION
            // ==========================================

            var donation =
                await _context.Donations
                    .FirstOrDefaultAsync(d =>
                        d.Id == id &&
                        d.UserId == user.Id);


            // ==========================================
            // CHECK DONATION
            // ==========================================

            if (donation == null)
            {
                return NotFound();
            }


            // ==========================================
            // CHECK CERTIFICATE
            // ==========================================

            if (string.IsNullOrWhiteSpace(
                donation.TaxCertificateNumber))
            {
                return NotFound(
                    "A tax certificate is not available for this donation.");
            }


            // ==========================================
            // DONOR NAME
            // ==========================================

            var donorName =
                user.FullName;

            if (string.IsNullOrWhiteSpace(
                donorName))
            {
                donorName =
                    user.Email ?? "Donor";
            }


            // ==========================================
            // DONATION DATE
            // ==========================================

            var donationDate =
                donation.DonationDate.ToLocalTime();


            // ==========================================
            // QUESTPDF LICENSE
            // ==========================================

            QuestPDF.Settings.License =
                LicenseType.Community;


            // ==========================================
            // CREATE PDF DOCUMENT
            // ==========================================

            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // A4 page
                        page.Size(PageSizes.A4);

                        // Page margins
                        page.Margin(50);

                        // Default text
                        page.DefaultTextStyle(
                            x => x.FontSize(12));


                        // ==================================
                        // HEADER
                        // ==================================

                        page.Header()
                            .Column(column =>
                            {
                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        "GIFT OF THE GIVERS")
                                    .Bold()
                                    .FontSize(24);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        "DISASTER RELIEF")
                                    .FontSize(12);

                                column.Item()
                                    .PaddingTop(15)
                                    .LineHorizontal(1);
                            });


                        // ==================================
                        // MAIN CONTENT
                        // ==================================

                        page.Content()
                            .PaddingTop(30)
                            .Column(column =>
                            {
                                column.Spacing(15);


                                // Certificate title

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        "TAX DONATION CERTIFICATE")
                                    .Bold()
                                    .FontSize(20);


                                // Thank you message

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        "Thank you for supporting " +
                                        "disaster relief and " +
                                        "communities in need.")
                                    .FontSize(11);


                                // ==================================
                                // CERTIFICATE NUMBER
                                // ==================================

                                column.Item()
                                    .PaddingTop(15)
                                    .AlignCenter()
                                    .Text(
                                        $"Certificate Number: " +
                                        $"{donation.TaxCertificateNumber}")
                                    .Bold()
                                    .FontSize(12);


                                // ==================================
                                // DONOR DETAILS
                                // ==================================

                                column.Item()
                                    .PaddingTop(15)
                                    .Text(
                                        "DONOR DETAILS")
                                    .Bold()
                                    .FontSize(14);


                                column.Item()
                                    .Border(1)
                                    .Padding(15)
                                    .Column(details =>
                                    {
                                        details.Spacing(8);


                                        details.Item()
                                            .Text(
                                                $"Donor Name: " +
                                                $"{donorName}");


                                        details.Item()
                                            .Text(
                                                $"Email: " +
                                                $"{user.Email}");


                                        details.Item()
                                            .Text(
                                                $"Donation Date: " +
                                                $"{donationDate:dd MMMM yyyy}");


                                        details.Item()
                                            .Text(
                                                $"Donation Time: " +
                                                $"{donationDate:HH:mm}");


                                        details.Item()
                                            .Text(
                                                $"Donation Type: " +
                                                $"{donation.DonationType}");


                                        details.Item()
                                            .Text(
                                                $"Donation Amount: " +
                                                $"{donation.Currency} " +
                                                $"{donation.Amount:N2}");


                                        details.Item()
                                            .Text(
                                                $"Currency: " +
                                                $"{donation.Currency}");
                                    });


                                // ==================================
                                // CERTIFICATION
                                // ==================================

                                column.Item()
                                    .PaddingTop(20)
                                    .Text(
                                        "CERTIFICATION")
                                    .Bold()
                                    .FontSize(14);


                                column.Item()
                                    .Text(
                                        "This certificate confirms " +
                                        "that the above donation was " +
                                        "recorded by Gift of the Givers " +
                                        "for disaster relief and " +
                                        "humanitarian support.");


                                // ==================================
                                // REFERENCE
                                // ==================================

                                column.Item()
                                    .PaddingTop(20)
                                    .Text(
                                        $"Certificate Reference: " +
                                        $"{donation.TaxCertificateNumber}")
                                    .Bold();


                                column.Item()
                                    .Text(
                                        $"Issued Date: " +
                                        $"{DateTime.Now:dd MMMM yyyy}");


                                // ==================================
                                // THANK YOU
                                // ==================================

                                column.Item()
                                    .PaddingTop(40)
                                    .AlignCenter()
                                    .Text(
                                        "Thank you for making a difference.")
                                    .Bold()
                                    .FontSize(13);
                            });


                        // ==================================
                        // FOOTER
                        // ==================================

                        page.Footer()
                            .AlignCenter()
                            .Text(
                                "Gift of the Givers - Donor Portal");
                    });
                });


            // ==========================================
            // GENERATE PDF
            // ==========================================

            var pdfBytes =
                document.GeneratePdf();


            // ==========================================
            // PDF FILE NAME
            // ==========================================

            var fileName =
                $"Tax-Certificate-" +
                $"{donation.TaxCertificateNumber}.pdf";


            // ==========================================
            // DOWNLOAD PDF
            // ==========================================

            return File(
                pdfBytes,
                "application/pdf",
                fileName);
        }


        // ==========================================
        // DONOR PROFILE - GET
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
        // DONOR PROFILE - POST
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

            if (string.IsNullOrWhiteSpace(
                fullName))
            {
                ModelState.AddModelError(
                    "fullName",
                    "Please enter your full name.");
            }


            // ==========================================
            // VALIDATE EMAIL
            // ==========================================

            if (string.IsNullOrWhiteSpace(
                email))
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

            user.FullName =
                fullName;


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
                        email);

                if (!emailResult.Succeeded)
                {
                    foreach (var error
                        in emailResult.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            error.Description);
                    }

                    return View(user);
                }


                // Keep username the same as email

                var usernameResult =
                    await _userManager.SetUserNameAsync(
                        user,
                        email);

                if (!usernameResult.Succeeded)
                {
                    foreach (var error
                        in usernameResult.Errors)
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

                await _signInManager.RefreshSignInAsync(
                    user);
            }
            else
            {
                foreach (var error
                    in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }
            }


            return View(user);
        }


        // ==========================================
        // DONOR LOGOUT
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