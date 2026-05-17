using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using ProiectWeb.Services;

namespace ProiectWeb.Pages.Client.Favorites
{
    [Authorize(Roles = "User")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RecommendationService _recommendationService;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RecommendationService recommendationService)
        {
            _context = context;
            _userManager = userManager;
            _recommendationService = recommendationService;
        }

        public List<Product> FavoriteProducts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                FavoriteProducts = await _context.Favorites
                    .Where(f => f.UserId == user.Id)
                    .Select(f => f.Product)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostRemoveAsync(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.ProductId == productId && f.UserId == user.Id);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetRecommendations(int productId)
        {
            try
            {
                // Obținem lista de obiecte Product direct
                var recommendations = await _recommendationService.GetHybridRecommendationsAsync(productId);

                // Returnăm direct lista; Serializatorul va recunoaște proprietățile Id, Name, Imagine etc.
                return new JsonResult(recommendations);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare recomandari: " + ex.Message);
                return new JsonResult(new List<object>());
            }
        }
    }
}