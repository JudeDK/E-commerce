using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System.IO;
using System.Threading.Tasks;

namespace ProiectWeb.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Products.FindAsync(id);

            if (Product == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // 1. Ștergem și imaginea de pe disc (Opțional, dar recomandat)
            if (!string.IsNullOrEmpty(product.Imagine) && product.Imagine != "default.jpg")
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/produse", product.Imagine);
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch
                    {
                        // Ignorăm eroarea dacă fișierul e blocat, continuăm ștergerea din DB
                    }
                }
            }

            // 2. Ștergem din baza de date
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // 3. Adăugăm notificare
            _context.Notifications.Add(new Notification
            {
                Message = $"Produsul '{product.Name}' a fost șters."
                // Date = DateTime.UtcNow // Decomentează dacă ai câmp de dată
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produsul a fost șters cu succes.";

            return RedirectToPage("/Admin/Products/Index");
        }
    }
}