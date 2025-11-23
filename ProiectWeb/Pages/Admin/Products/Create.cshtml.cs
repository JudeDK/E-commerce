using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System.IO;
using System.Threading.Tasks;

namespace ProiectWeb.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile ImagineFisier { get; set; } // pentru upload imagine

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Salvează fișierul dacă există
            if (ImagineFisier != null && ImagineFisier.Length > 0)
            {
                // Creează folderul dacă nu există
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/produse");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileName = Path.GetFileName(ImagineFisier.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImagineFisier.CopyToAsync(stream);
                }

                Product.Imagine = fileName; // salvează doar numele fișierului în DB
            }

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            _context.Notifications.Add(new Notification
            {
                Message = $"Produsul '{Product.Name}' a fost adăugat."
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produsul a fost adăugat cu succes.";

            return RedirectToPage("/Admin/Products/Index");
        }
    }
}
