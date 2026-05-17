// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ProiectWeb.Models;
using ProiectWeb.Services;

namespace ProiectWeb.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly TwoFactorService _twoFactorService;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger,
            TwoFactorService twoFactorService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _twoFactorService = twoFactorService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Verificăm parola fără a finaliza loginul
            var pwCheck = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, false);

            if (pwCheck.Succeeded)
            {
                // Dacă are 2FA activ, declanșăm fluxul
                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    // 1. Trimitem codul prin email
                    var code = await _twoFactorService.SendTwoFactorCodeAsync(user);

                    HttpContext.Session.SetString("2FA_Code", code);
                    HttpContext.Session.SetString("2FA_UserId", user.Id);


                    // 2. Salvăm userId în sesiune
                    HttpContext.Session.SetString("2FA_UserId", user.Id);

                    // 3. SPUNEM Identity că este în modul 2FA
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl });
                }

                // Dacă nu are 2FA → login direct
                await _signInManager.SignInAsync(user, Input.RememberMe);
                return LocalRedirect(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

    }
}
