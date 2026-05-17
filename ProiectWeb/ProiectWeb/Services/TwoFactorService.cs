using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using ProiectWeb.Models;

namespace ProiectWeb.Services
{
    public class TwoFactorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public TwoFactorService(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // Trimite codul 2FA pe email
        public async Task<string> SendTwoFactorCodeAsync(ApplicationUser user)
        {
            var code = new Random().Next(100000, 999999).ToString();

            var body = $@"
                    <div style='width:100%; background:#f5f7f8; padding:30px 0; font-family:Arial, sans-serif;'>
                    <div style='max-width:600px; margin:0 auto; background:white; padding:40px; border-radius:12px; 
                            box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

                    <h2 style='color:#1a73e8; font-size:24px; margin-bottom:20px;'>
                        Autentificare în doi pași
                    </h2>

                    <p style='font-size:16px; color:#444; margin-bottom:10px;'>
                        Codul dumneavoastră de verificare este:
                    </p>

                    <div style='font-size:36px; font-weight:bold; color:#0f9d58; margin:25px 0;'>
                    {code}
                    </b>";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Cod 2FA",
                body);

            return code;
        }

    }
}
