using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using ProiectWeb.Models;

namespace ProiectWeb.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginWith2faModel(SignInManager<ApplicationUser> signInManager,
                                 UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        [BindProperty]
        public string TwoFactorCode { get; set; }


        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetString("2FA_UserId");
            var correctCode = HttpContext.Session.GetString("2FA_Code");

            if (userId == null || correctCode == null)
            {
                ModelState.AddModelError(string.Empty, "Sesiunea a expirat.");
                return Page();
            }

            if (TwoFactorCode.Trim() != correctCode)
            {
                ModelState.AddModelError("", "Cod invalid.");
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);

            // LOGIN MANUAL după verificare reușită
            await _signInManager.SignInAsync(user, false);

            HttpContext.Session.Remove("2FA_UserId");
            HttpContext.Session.Remove("2FA_Code");

            return LocalRedirect(ReturnUrl);
        }

    }
}
