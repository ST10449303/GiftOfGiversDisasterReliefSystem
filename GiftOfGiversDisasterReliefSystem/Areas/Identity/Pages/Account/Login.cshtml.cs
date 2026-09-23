using GiftOfGiversDisasterReliefSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GiftOfGiversDisasterReliefSystem.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }


        // ==========================================
        // Login Input
        // ==========================================

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();


        public string? ReturnUrl { get; set; }


        public class InputModel
        {
            [Required(ErrorMessage = "Please select a role.")]
            [Display(Name = "Select Panel")]
            public string Role { get; set; } = string.Empty;


            [Required(ErrorMessage = "Please enter your email address.")]
            [EmailAddress(
                ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = string.Empty;


            [Required(ErrorMessage = "Please enter your password.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;


            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }


        // ==========================================
        // Display Login Page
        // ==========================================

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }


        // ==========================================
        // Process Login
        // ==========================================

        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            ReturnUrl = returnUrl;


            // ==========================================
            // Validate Form
            // ==========================================

            if (!ModelState.IsValid)
            {
                return Page();
            }


            // ==========================================
            // Validate Selected Role
            // ==========================================

            if (Input.Role != "Donor" &&
                Input.Role != "Employee")
            {
                ModelState.AddModelError(
                    nameof(Input.Role),
                    "Please select a valid panel.");

                return Page();
            }


            // ==========================================
            // Find User By Email
            // ==========================================

            var user = await _userManager.FindByEmailAsync(
                Input.Email);


            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email, password or panel.");

                return Page();
            }


            // ==========================================
            // Check Selected Role
            // ==========================================

            var hasRole = await _userManager.IsInRoleAsync(
                user,
                Input.Role);


            if (!hasRole)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The selected panel does not match this account.");

                return Page();
            }


            // ==========================================
            // Check Password
            // ==========================================

            var result =
                await _signInManager.CheckPasswordSignInAsync(
                    user,
                    Input.Password,
                    lockoutOnFailure: true);


            // ==========================================
            // Successful Login
            // ==========================================

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(
                    user,
                    isPersistent: Input.RememberMe);


                // ==========================================
                // Employee Panel
                // ==========================================

                if (Input.Role == "Employee")
                {
                    return LocalRedirect("/Employee");
                }


                // ==========================================
                // Donor Panel
                // ==========================================

                if (Input.Role == "Donor")
                {
                    return LocalRedirect("/Donor");
                }
            }


            // ==========================================
            // Account Locked
            // ==========================================

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This account has been locked. Please try again later.");

                return Page();
            }


            // ==========================================
            // Login Failed
            // ==========================================

            ModelState.AddModelError(
                string.Empty,
                "Invalid email, password or panel.");

            return Page();
        }
    }
}