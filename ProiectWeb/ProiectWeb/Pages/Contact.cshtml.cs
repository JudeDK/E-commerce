using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProiectWeb.Pages;

public class ContactModel : PageModel
{
    private const string ContactRecipient = "raresmarian3344@gmail.com";

    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IEmailSender _emailSender;

    public ContactModel(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [BindProperty]
    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Numele trebuie să aibă între 2 și 100 de caractere.")]
    [Display(Name = "Nume")]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Emailul este obligatoriu.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Mesajul este obligatoriu.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mesajul trebuie să aibă între 10 și 2000 de caractere.")]
    [Display(Name = "Mesaj")]
    public string Message { get; set; } = string.Empty;

    public bool Submitted { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!EmailRegex.IsMatch(Email?.Trim() ?? string.Empty))
        {
            ModelState.AddModelError(nameof(Email),
                "Introduceți un email valid (ex: nume@gmail.com, nume@yahoo.com, nume@outlook.com).");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var subject = $"Contact E-commerce — {Name.Trim()}";
        var body = $@"
            <h3>Mesaj nou de pe formularul de contact</h3>
            <p><strong>Nume:</strong> {System.Net.WebUtility.HtmlEncode(Name.Trim())}</p>
            <p><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(Email.Trim())}</p>
            <p><strong>Mesaj:</strong></p>
            <p>{System.Net.WebUtility.HtmlEncode(Message.Trim()).Replace("\n", "<br/>")}</p>";

        try
        {
            await _emailSender.SendEmailAsync(ContactRecipient, subject, body);
            Submitted = true;
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Nu am putut trimite mesajul. Încercați din nou mai târziu.");
        }

        return Page();
    }
}
