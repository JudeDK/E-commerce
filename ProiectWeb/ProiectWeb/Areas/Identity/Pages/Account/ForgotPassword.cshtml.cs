// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ProiectWeb.Models;

namespace ProiectWeb.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Resetare parolă - E-Commerce",
                        $@"
                            <div style='width:100%; background:#f5f7f8; padding:30px 0; font-family:Arial, sans-serif;'>
                                <div style='max-width:600px; margin:0 auto; background:white; padding:40px; border-radius:12px;
                                            box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

                                    <h2 style='color:#2c3e50; font-size:24px; margin-bottom:20px;'>
                                        Resetare parolă
                                    </h2>

                                    <p style='font-size:15px; color:#444;'>
                                        Ați solicitat resetarea parolei pentru contul dvs. E-Commerce.
                                    </p>

                                    <p style='font-size:15px; color:#444;'>
                                        👉 Dați click pe butonul de mai jos pentru a seta o parolă nouă:
                                    </p>

                                    <p style='margin-top:25px;'>
                                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                                            style='padding:12px 22px; background:#1a73e8; color:white; text-decoration:none;
                                                   font-size:16px; border-radius:6px; font-weight:bold;'>
                                            Resetează Parola
                                        </a>
                                    </p>

                                    <p style='font-size:13px; color:#7f8c8d; margin-top:30px;'>
                                        Dacă nu dumneavoastră ați solicitat resetarea parolei, puteți ignora acest mesaj.
                                    </p>
                                </div>
                            </div>
                        ");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
