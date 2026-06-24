using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.RazorPages;

using ProiectWeb.Data;

using ProiectWeb.Models;

using ProiectWeb.Services;

using System;

using System.IO;

using System.Threading.Tasks;



namespace ProiectWeb.Pages.Admin.Products

{

    [Authorize(Roles = "Admin")]

    public class CreateModel : PageModel

    {

        private readonly ApplicationDbContext _context;

        private readonly ProductImageService _images;



        public CreateModel(ApplicationDbContext context, ProductImageService images)

        {

            _context = context;

            _images = images;

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



            if (ImagineFisier != null && ImagineFisier.Length > 0)

            {

                Directory.CreateDirectory(_images.ImaginiDirectory);

                var fileName = ProductImageService.SanitizeFileName(Product.Name);

                var filePath = _images.GetUploadPath(Product.Name);



                using (var stream = new FileStream(filePath, FileMode.Create))

                {

                    await ImagineFisier.CopyToAsync(stream);

                }



                Product.Imagine = fileName;

            }

            else

            {

                Product.Imagine = _images.ResolveFileName(Product.Name);

            }



            Product.AddedDate = DateTime.UtcNow;

            if (Product.AcquisitionCost <= 0)

                Product.AcquisitionCost = Math.Round(Product.Price * 0.65m, 2);



            _context.Products.Add(Product);

            await _context.SaveChangesAsync();



            _context.Notifications.Add(new Notification

            {

                Message = $"Produsul '{Product.Name}' a fost adăugat.",

            });

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = "Produsul a fost adăugat cu succes.";



            return RedirectToPage("/Admin/Products/Index");

        }

    }

}

