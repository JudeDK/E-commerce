// highlight: RegisterModel.cs (versiunea finală modificată complet)
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
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Nickname is required")]
            [Display(Name = "Nickname")]
            public string Nickname { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.Nickname = Input.Nickname;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // ACTIVĂM 2FA pentru user
                    await _userManager.SetTwoFactorEnabledAsync(user, true);

                    // SETĂM PROVIDERUL 2FA pe Email
                    await _userManager.SetAuthenticationTokenAsync(
                        user,
                        "Default",
                        "TwoFactorProvider",
                        "Email"
                    );

                    // GENERĂM LINKUL DE CONFIRMARE EMAIL
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);


                    // TRIMITEM EMAIL PERSONALIZAT
                    await _emailSender.SendEmailAsync(Input.Email,
                        "Confirmare cont - E-Commerce",
                        $@"
                            <h2 style='font-family:Arial; color:#2c3e50;'>Mulțumesc, {Input.Nickname}, pentru contul creat la E-Commerce!</h2>

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

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try { return Activator.CreateInstance<ApplicationUser>(); }
            catch
            {
                throw new InvalidOperationException(
                    $"Can't create '{nameof(ApplicationUser)}'. Ensure it has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("This requires a user store with email support.");

            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
