using GiftOfGiversDisasterReliefSystem.Data;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfGiversDisasterReliefSystem.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ==========================================
        // CONTACT PAGE
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ==========================================
        // SEND CONTACT MESSAGE
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            string name,
            string email,
            string subject,
            string message)
        {
            // ==========================================
            // VALIDATE NAME
            // ==========================================

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(
                    "Name",
                    "Please enter your full name.");
            }


            // ==========================================
            // VALIDATE EMAIL
            // ==========================================

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Please enter your email address.");
            }


            // ==========================================
            // VALIDATE SUBJECT
            // ==========================================

            if (string.IsNullOrWhiteSpace(subject))
            {
                ModelState.AddModelError(
                    "Subject",
                    "Please enter a subject.");
            }


            // ==========================================
            // VALIDATE MESSAGE
            // ==========================================

            if (string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(
                    "Message",
                    "Please enter your message.");
            }


            // ==========================================
            // RETURN TO CONTACT PAGE IF INVALID
            // ==========================================

            if (!ModelState.IsValid)
            {
                return View("Index");
            }


            // ==========================================
            // CREATE CONTACT MESSAGE
            // ==========================================

            var contactMessage = new ContactMessage
            {
                Name = name.Trim(),

                Email = email.Trim(),

                Subject = subject.Trim(),

                Message = message.Trim(),

                DateSent = DateTime.Now,

                IsRead = false
            };


            // ==========================================
            // SAVE MESSAGE TO DATABASE
            // ==========================================

            _context.ContactMessages.Add(contactMessage);

            await _context.SaveChangesAsync();


            // ==========================================
            // STORE NAME FOR CONFIRMATION PAGE
            // ==========================================

            TempData["ContactName"] = contactMessage.Name;


            // ==========================================
            // REDIRECT TO CONFIRMATION
            // ==========================================

            return RedirectToAction("Confirmation");
        }


        // ==========================================
        // MESSAGE CONFIRMATION
        // ==========================================

        [HttpGet]
        public IActionResult Confirmation()
        {
            ViewBag.Name =
                TempData["ContactName"] ?? "there";

            return View();
        }
    }
}