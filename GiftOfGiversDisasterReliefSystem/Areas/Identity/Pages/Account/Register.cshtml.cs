using GiftOfGiversDisasterReliefSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GiftOfGiversDisasterReliefSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        // ==========================================
        // INPUT
        // ==========================================

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();


        // ==========================================
        // REGISTRATION FIELDS
        // ==========================================

        public class InputModel
        {
            // Account Type

            [Required(ErrorMessage = "Please select an account type.")]
            [Display(Name = "Account Type")]
            public string Role { get; set; } = string.Empty;


            // Full Name

            [Required(ErrorMessage = "Please enter your full name.")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;


            // Email

            [Required(ErrorMessage = "Please enter your email address.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = string.Empty;


            // Password

            [Required(ErrorMessage = "Please enter a password.")]
            [DataType(DataType.Password)]
            [StringLength(
                100,
                ErrorMessage = "The password must be at least {2} characters long.",
                MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;


            // Confirm Password

            [Required(ErrorMessage = "Please confirm your password.")]
            [DataType(DataType.Password)]
            [Compare(
                "Password",
                ErrorMessage = "The passwords do not match.")]
            [Display(Name = "Confirm Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }


        // ==========================================
        // DISPLAY REGISTRATION PAGE
        // ==========================================

        public void OnGet()
        {
        }


        // ==========================================
        // PROCESS REGISTRATION
        // ==========================================

        public async Task<IActionResult> OnPostAsync()
        {
            // Check validation

            if (!ModelState.IsValid)
            {
                return Page();
            }


            // ==========================================
            // CHECK SELECTED ROLE
            // ==========================================

            if (Input.Role != "Donor" &&
                Input.Role != "Employee")
            {
                ModelState.AddModelError(
                    nameof(Input.Role),
                    "Please select either Donor or Employee.");

                return Page();
            }


            // ==========================================
            // CREATE ROLE IF IT DOES NOT EXIST
            // ==========================================

            if (!await _roleManager.RoleExistsAsync(Input.Role))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole(Input.Role));

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return Page();
                }
            }


            // ==========================================
            // CREATE APPLICATION USER
            // ==========================================

            var user = new ApplicationUser
            {
                FullName = Input.FullName,
                UserName = Input.Email,
                Email = Input.Email
            };


            var result = await _userManager.CreateAsync(
                user,
                Input.Password);


            // ==========================================
            // CHECK USER CREATION
            // ==========================================

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }


            // ==========================================
            // ASSIGN SELECTED ROLE
            // ==========================================

            var roleAssignmentResult =
                await _userManager.AddToRoleAsync(
                    user,
                    Input.Role);


            if (!roleAssignmentResult.Succeeded)
            {
                foreach (var error in roleAssignmentResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                // Delete the user if role assignment fails

                await _userManager.DeleteAsync(user);

                return Page();
            }


            // ==========================================
            // REGISTRATION SUCCESSFUL
            // ==========================================
            //
            // Do NOT automatically sign the user in.
            //
            // Send the user to the Login page.
            //
            // ==========================================

            return RedirectToPage(
                "/Account/Login",
                new
                {
                    area = "Identity"
                });
        }
    }
}