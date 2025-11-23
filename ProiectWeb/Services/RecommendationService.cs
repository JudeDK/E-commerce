using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;

namespace ProiectWeb.Services
{
    public class RecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Product? GetRecommendationForUser(string userId)
        {
            // Produse cumpărate de user
            var userProductIds = _context.Orders
                .Where(o => o.UserId == userId)
                .SelectMany(o => o.Items)
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            // Categoriile corespunzătoare
            var userCategories = _context.Products
                .Where(p => userProductIds.Contains(p.Id))
                .Select(p => p.Category)
                .Distinct()
                .ToList();

            // Produse din acele categorii cumpărate de alții, dar nu de userul curent
            var recommended = _context.Orders
                .Where(o => o.UserId != userId)
                .SelectMany(o => o.Items)
                .Where(i => userCategories.Contains(i.Product.Category) && !userProductIds.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.First().Product)
                .FirstOrDefault();

            return recommended;
        }
    }
}
