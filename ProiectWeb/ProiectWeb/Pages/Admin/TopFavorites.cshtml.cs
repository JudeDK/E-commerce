using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProiectWeb.Areas.Admin.Pages
{
    public class TopFavoritesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TopFavoritesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Definim o clasă mică pentru a ține datele statistice
        public class ProductStat
        {
            public string Name { get; set; }
            public string Imagine { get; set; }
            public int Count { get; set; }
            public decimal Price { get; set; }
        }

        public List<ProductStat> PopularProducts { get; set; } = new();

        public async Task OnGetAsync()
        {
            PopularProducts = await _context.Favorites
                .GroupBy(f => f.ProductId) // Grupăm după ID-ul produsului
                .Select(g => new ProductStat
                {
                    Name = g.First().Product.Name,
                    Imagine = g.First().Product.Imagine,
                    Price = g.First().Product.Price,
                    Count = g.Count() // Numărăm câți utilizatori îl au la favorite
                })
                .OrderByDescending(ps => ps.Count) // Sortăm de la mare la mic
                .ToListAsync();
        }
    }
}