using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // Necesar pentru IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProiectWeb.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile ImagineFisier { get; set; } // Adăugat pentru a putea schimba poza

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Products.FindAsync(id);

            if (Product == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validare de bază
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Încărcăm produsul existent din baza de date pentru a nu pierde date
            var productToUpdate = await _context.Products.FindAsync(Product.Id);

            if (productToUpdate == null)
            {
                return NotFound();
            }

            // 2. Actualizăm valorile
            productToUpdate.Name = Product.Name;
            productToUpdate.Price = Product.Price;
            productToUpdate.Quantity = Product.Quantity;
            productToUpdate.Category = Product.Category;
            productToUpdate.Description = Product.Description;

            // Nu modificăm AddedDate, o păstrăm pe cea originală
            // Dacă vrei "LastModified", ai putea adăuga un câmp nou și seta = DateTime.UtcNow

            // 3. Gestionare Imagine (Dacă s-a încărcat una nouă)
            if (ImagineFisier != null && ImagineFisier.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/produse");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                // Opțional: Șterge imaginea veche dacă nu e cea default
                if (!string.IsNullOrEmpty(productToUpdate.Imagine) && productToUpdate.Imagine != "default.jpg")
                {
                    var oldPath = Path.Combine(uploadDir, productToUpdate.Imagine);
                    if (System.IO.File.Exists(oldPath))
                    {
                        try { System.IO.File.Delete(oldPath); } catch { /* Ignoră eroare ștergere */ }
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImagineFisier.FileName);
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImagineFisier.CopyToAsync(stream);
                }

                productToUpdate.Imagine = uniqueFileName;
            }

            try
            {
                await _context.SaveChangesAsync();

                _context.Notifications.Add(new Notification
                {
                    Message = $"Produsul '{productToUpdate.Name}' a fost modificat."
                    // Date = DateTime.UtcNow // Decomentează dacă ai câmp de dată în Notification
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Produsul a fost editat cu succes.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(Product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/Admin/Products/Index");
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}