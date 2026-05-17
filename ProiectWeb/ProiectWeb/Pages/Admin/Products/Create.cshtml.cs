using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System; // Necesar pentru DateTime
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
        public IFormFile ImagineFisier { get; set; }

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
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/produse");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                // Generăm un nume unic ca să nu suprascriem fișiere cu același nume
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImagineFisier.FileName);
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImagineFisier.CopyToAsync(stream);
                }

                Product.Imagine = uniqueFileName;
            }
            else
            {
                // Dacă nu se încarcă nicio imagine, punem una default sau null
                Product.Imagine = "default.jpg";
            }

            // ✅ CRITIC PENTRU POSTGRESQL: Folosim UTC
            Product.AddedDate = DateTime.UtcNow;

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            // Adăugăm notificare
            _context.Notifications.Add(new Notification
            {
                Message = $"Produsul '{Product.Name}' a fost adăugat.",
                // Dacă modelul Notification are un câmp Date/CreatedAt, asigură-te că e setat tot cu UtcNow
                // Date = DateTime.UtcNow 
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produsul a fost adăugat cu succes.";

            return RedirectToPage("/Admin/Products/Index");
        }
    }
}