// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ProiectWeb.Models;

namespace ProiectWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            // Dacă utilizatorul nu există → nu dezvăluim, dar nici NU trimitem email
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Dacă adresa există, un email a fost trimis.");
                return Page();
            }

            // ❗ Dacă emailul este deja confirmat → NU mai trimitem email
            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Acest email este deja confirmat.");
                return Page();
            }

            // Dacă emailul NU este confirmat → generăm confirmarea ca înainte
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Confirmare cont - E-Commerce",
                $@"
            <h2 style='font-family:Arial; color:#2c3e50;'>Mulțumesc, {user.Nickname}, pentru contul creat la E-Commerce!</h2>

            <p style='font-family:Arial; font-size:15px;'>
                Mai rămâne un singur pas pentru a vă pregăti contul.
            </p>

            <p style='font-family:Arial; font-size:15px;'>
                👉 Dați click pe linkul următor pentru a finaliza partea de înregistrare și a confirma emailul:
            </p>

            <p>
                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                   style='padding:12px 20px; color:white; background:#27ae60;
                          text-decoration:none; border-radius:6px; font-size:16px;'>
                    Confirmă Contul
                </a>
            </p>

            <p style='font-family:Arial; font-size:13px; color:#7f8c8d;'>
                Dacă nu dumneavoastră ați creat acest cont, puteți ignora acest email.
            </p>
        ");

            ModelState.AddModelError(string.Empty, "Emailul de confirmare a fost trimis din nou.");
            return Page();
        }

    }
}
