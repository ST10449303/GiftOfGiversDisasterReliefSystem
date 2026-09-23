// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using GiftOfGiversDisasterReliefSystem.Data;

namespace GiftOfGiversDisasterReliefSystem.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }


    // ==========================================
    // INPUT MODEL
    // ==========================================

    [BindProperty]
    public InputModel Input { get; set; } = default!;


    public class InputModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }


    // ==========================================
    // FORGOT PASSWORD
    // ==========================================

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }


        // ==========================================
        // FIND USER
        // ==========================================

        var user = await _userManager.FindByEmailAsync(Input.Email);


        // ==========================================
        // ACCOUNT NOT FOUND
        // ==========================================

        if (user == null)
        {
            ModelState.AddModelError(
                "",
                "No account was found with this email address.");

            return Page();
        }


        // ==========================================
        // GENERATE PASSWORD RESET TOKEN
        // ==========================================

        var code =
            await _userManager.GeneratePasswordResetTokenAsync(user);


        // ==========================================
        // ENCODE TOKEN
        // ==========================================

        code = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(code));


        // ==========================================
        // GO DIRECTLY TO RESET PASSWORD PAGE
        // ==========================================

        return RedirectToPage(
            "./ResetPassword",
            new
            {
                code = code,
                email = Input.Email
            });
    }
}