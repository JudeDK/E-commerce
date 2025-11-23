using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProiectWeb.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        private const int PageSize = 15; // ✅ 15 produse/pagină

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            await LoadProducts(SearchString, PageNumber);
        }

        // ✅ căutare live - returnează doar partial-ul
        public async Task<IActionResult> OnGetSearchAsync(string searchString, int pageNumber = 1)
        {
            await LoadProducts(searchString, pageNumber);
            return Partial("_ProductListPartial", this);
        }

        // ✅ paginare - returnează doar partial-ul
        public async Task<IActionResult> OnGetPageAsync(int pageNumber)
        {
            await LoadProducts(SearchString, pageNumber);
            return Partial("_ProductListPartial", this);
        }

        private async Task LoadProducts(string searchString, int pageNumber)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{searchString.ToLower()}%"));
            }

            int totalProducts = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalProducts / (double)PageSize);

            Products = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            PageNumber = pageNumber;
        }
    }
}
