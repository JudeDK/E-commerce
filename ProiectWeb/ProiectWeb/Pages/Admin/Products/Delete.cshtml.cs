using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProiectWeb.Data;
using ProiectWeb.Models;
using ProiectWeb.Services;
using System.IO;
using System.Threading.Tasks;

namespace ProiectWeb.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductImageService _images;

        public DeleteModel(ApplicationDbContext context, ProductImageService images)
        {
            _context = context;
            _images = images;
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

            if (!string.IsNullOrEmpty(product.Imagine)
                && product.Imagine != ProductImageService.NotFoundFileName
                && _images.FileExists(product.Imagine))
            {
                var filePath = Path.Combine(_images.ImaginiDirectory, product.Imagine);
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); }
                    catch { /* Ignorăm eroarea dacă fișierul e blocat */ }
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _context.Notifications.Add(new Notification
            {
                Message = $"Produsul '{product.Name}' a fost șters."
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produsul a fost șters cu succes.";

            return RedirectToPage("/Admin/Products/Index");
        }
    }
}
