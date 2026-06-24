using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.EntityFrameworkCore;

using ProiectWeb.Data;

using ProiectWeb.Models;

using ProiectWeb.Services;

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

        private readonly ProductImageService _images;



        public EditModel(ApplicationDbContext context, ProductImageService images)

        {

            _context = context;

            _images = images;

        }



        [BindProperty]

        public Product Product { get; set; } = new();



        [BindProperty]

        public IFormFile ImagineFisier { get; set; }



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

            if (!ModelState.IsValid)

            {

                return Page();

            }



            var productToUpdate = await _context.Products.FindAsync(Product.Id);



            if (productToUpdate == null)

            {

                return NotFound();

            }



            productToUpdate.Name = Product.Name;

            productToUpdate.Price = Product.Price;

            productToUpdate.Quantity = Product.Quantity;

            productToUpdate.Category = Product.Category;

            productToUpdate.Description = Product.Description;

            productToUpdate.AcquisitionCost = Product.AcquisitionCost > 0

                ? Product.AcquisitionCost

                : Math.Round(Product.Price * 0.65m, 2);



            if (ImagineFisier != null && ImagineFisier.Length > 0)

            {

                Directory.CreateDirectory(_images.ImaginiDirectory);



                var newFileName = ProductImageService.SanitizeFileName(productToUpdate.Name);

                var oldName = productToUpdate.Imagine;



                if (!string.IsNullOrEmpty(oldName)

                    && oldName != newFileName

                    && oldName != ProductImageService.NotFoundFileName

                    && _images.FileExists(oldName))

                {

                    try { System.IO.File.Delete(Path.Combine(_images.ImaginiDirectory, oldName)); }

                    catch { /* Ignoră eroare ștergere */ }

                }



                var filePath = _images.GetUploadPath(productToUpdate.Name);

                using (var stream = new FileStream(filePath, FileMode.Create))

                {

                    await ImagineFisier.CopyToAsync(stream);

                }



                productToUpdate.Imagine = newFileName;

            }

            else

            {

                productToUpdate.Imagine = _images.ResolveFileName(productToUpdate.Name, productToUpdate.Imagine);

            }



            try

            {

                await _context.SaveChangesAsync();

            }

            catch (DbUpdateConcurrencyException)

            {

                if (!await _context.Products.AnyAsync(e => e.Id == Product.Id))

                    return NotFound();

                throw;

            }



            TempData["SuccessMessage"] = "Produsul a fost actualizat cu succes.";

            return RedirectToPage("/Admin/Products/Index");

        }

    }

}

