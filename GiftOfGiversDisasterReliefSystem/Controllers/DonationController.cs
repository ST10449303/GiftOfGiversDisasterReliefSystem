using GiftOfGiversDisasterReliefSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GiftOfGiversDisasterReliefSystem.Controllers
{
    public class DonationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public DonationController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }


        // ==========================================
        // PUBLIC DONATION PAGE
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ==========================================
        // PROCESS PUBLIC DONATION
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            decimal amount,
            string donationType,
            string currency,
            bool anonymous = true)
        {
            // ==========================================
            // VALIDATE AMOUNT
            // ==========================================

            if (amount <= 0)
            {
                ModelState.AddModelError(
                    "amount",
                    "Please enter a valid donation amount.");

                return View("Index");
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

                return View("Index");
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

                return View("Index");
            }


            // ==========================================
            // GET LOGGED-IN USER
            // ==========================================

            var user =
                await _userManager.GetUserAsync(User);


            // ==========================================
            // CREATE DONATION
            // ==========================================

            var donation = new Donation
            {
                Amount = amount,
                DonationType = donationType,
                Currency = currency,
                IsAnonymous = anonymous,
                DonationDate = DateTime.UtcNow
            };


            // ==========================================
            // HANDLE DONOR INFORMATION
            // ==========================================

            if (anonymous)
            {
                // Anonymous donation

                donation.UserId = null;

                ViewBag.DonorName = "Anonymous Donor";
                ViewBag.DonorEmail = null;
            }
            else
            {
                // Named donation

                if (user != null)
                {
                    donation.UserId = user.Id;

                    ViewBag.DonorName =
                        !string.IsNullOrWhiteSpace(user.FullName)
                            ? user.FullName
                            : user.Email ?? "Donor";

                    ViewBag.DonorEmail =
                        user.Email;
                }
                else
                {
                    // Public user who selected
                    // non-anonymous but is not logged in.

                    ModelState.AddModelError(
                        "",
                        "Please log in to make a non-anonymous donation.");

                    return View("Index");
                }
            }


            // ==========================================
            // SAVE DONATION TO DATABASE
            // ==========================================

            _context.Donations.Add(donation);

            await _context.SaveChangesAsync();


            // ==========================================
            // SEND INFORMATION TO CONFIRMATION
            // ==========================================

            ViewBag.Amount =
                donation.Amount;

            ViewBag.DonationType =
                donation.DonationType;

            ViewBag.Currency =
                donation.Currency;

            ViewBag.Anonymous =
                donation.IsAnonymous;

            ViewBag.DonationDate =
                donation.DonationDate;


            // ==========================================
            // SHOW CONFIRMATION
            // ==========================================

            return View("Confirmation", donation);
        }


        // ==========================================
        // SUPPORT FOR FORMS POSTING TO INDEX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            decimal amount,
            string donationType,
            string currency,
            bool anonymous = true)
        {
            return await Submit(
                amount,
                donationType,
                currency,
                anonymous);
        }


        // ==========================================
        // DONOR DONATION PAGE
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> DonorDonation()
        {
            // ==========================================
            // CHECK LOGIN
            // ==========================================

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = "/Donor/Donate"
                    });
            }


            // ==========================================
            // CHECK DONOR ROLE
            // ==========================================

            if (!User.IsInRole("Donor"))
            {
                return Forbid();
            }


            // ==========================================
            // GET DONOR
            // ==========================================

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity"
                    });
            }


            ViewBag.DonorName =
                user.FullName;

            ViewBag.DonorEmail =
                user.Email;


            return View("DonorDonation", user);
        }


        // ==========================================
        // PROCESS DONOR DONATION
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DonorDonation(
            decimal amount,
            string donationType,
            string currency)
        {
            // ==========================================
            // CHECK LOGIN
            // ==========================================

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = "/Donor/Donate"
                    });
            }


            // ==========================================
            // CHECK DONOR ROLE
            // ==========================================

            if (!User.IsInRole("Donor"))
            {
                return Forbid();
            }


            // ==========================================
            // GET DONOR
            // ==========================================

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity"
                    });
            }


            // ==========================================
            // VALIDATE AMOUNT
            // ==========================================

            if (amount <= 0)
            {
                ModelState.AddModelError(
                    "amount",
                    "Please enter a valid donation amount.");

                ViewBag.DonorName =
                    user.FullName;

                ViewBag.DonorEmail =
                    user.Email;

                return View("DonorDonation", user);
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

                ViewBag.DonorName =
                    user.FullName;

                ViewBag.DonorEmail =
                    user.Email;

                return View("DonorDonation", user);
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

                ViewBag.DonorName =
                    user.FullName;

                ViewBag.DonorEmail =
                    user.Email;

                return View("DonorDonation", user);
            }


            // ==========================================
            // CALL AZURE FUNCTION
            // TO GENERATE TAX CERTIFICATE
            // ==========================================

            var functionUrl =
                "http://localhost:7071/api/GenerateTaxCertificate" +
                $"?donorName={Uri.EscapeDataString(user.FullName ?? "Donor")}" +
                $"&amount={amount}" +
                $"&currency={currency}";

            var functionResponse =
                await _httpClient.GetFromJsonAsync<TaxCertificateResponse>(
                    functionUrl);

            var certificateNumber =
                functionResponse?.CertificateNumber
                ?? $"GOG-{DateTime.UtcNow:yyyyMMddHHmmss}";


            // ==========================================
            // CREATE DONOR DONATION
            // ==========================================

            var donation = new Donation
            {
                UserId = user.Id,
                Amount = amount,
                DonationType = donationType,
                Currency = currency,
                IsAnonymous = false,
                DonationDate = DateTime.UtcNow,
                TaxCertificateNumber = certificateNumber
            };


            // ==========================================
            // SAVE DONATION
            // ==========================================

            _context.Donations.Add(donation);

            await _context.SaveChangesAsync();


            // ==========================================
            // SEND INFORMATION TO CONFIRMATION
            // ==========================================

            ViewBag.Amount =
                donation.Amount;

            ViewBag.DonationType =
                donation.DonationType;

            ViewBag.Currency =
                donation.Currency;

            ViewBag.Anonymous =
                donation.IsAnonymous;

            ViewBag.DonorName =
                user.FullName;

            ViewBag.DonorEmail =
                user.Email;

            ViewBag.CertificateNumber =
                donation.TaxCertificateNumber;

            ViewBag.DonationDate =
                donation.DonationDate;


            // ==========================================
            // SHOW CONFIRMATION
            // ==========================================

            return View("Confirmation", donation);
        }
    }


    // ==========================================
    // AZURE FUNCTION RESPONSE MODEL
    // ==========================================

    public class TaxCertificateResponse
    {
        public string Message { get; set; } = string.Empty;

        public string CertificateNumber { get; set; } = string.Empty;

        public string DonorName { get; set; } = string.Empty;

        public string DonationAmount { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public string GeneratedDate { get; set; } = string.Empty;
    }
}