using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using ProiectWeb.Services;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace ProiectWeb.Pages.Client.Products
{
    [Authorize(Roles = "User")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly RecommendationService _recommendationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, RecommendationService recommendationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _recommendationService = recommendationService;
            _userManager = userManager;
        }

        public List<Product> Products { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<int> UserFavoriteIds { get; set; } = new();
        public Product? RecommendedProduct { get; set; }

        [BindProperty(SupportsGet = true)] public string SearchString { get; set; }
        [BindProperty(SupportsGet = true)] public string SortOrder { get; set; }
        public int PageSize { get; set; } = 15;
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            
            await LoadProducts(SearchString, SortOrder, null, null, null);
        }

        public async Task<IActionResult> OnPostToggleFavoriteAsync(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingFav = await _context.Favorites
                .FirstOrDefaultAsync(f => f.ProductId == productId && f.UserId == user.Id);

            bool isAdded;
            if (existingFav != null)
            {
                _context.Favorites.Remove(existingFav);
                isAdded = false;
            }
            else
            {
                _context.Favorites.Add(new Favorite { ProductId = productId, UserId = user.Id });
                isAdded = true;
            }

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true, isFavorite = isAdded });
        }

        public async Task<JsonResult> OnGetRecommendations(int productId)
        {
            try
            {
                var final = await _recommendationService.GetHybridRecommendationsAsync(productId);
                return new JsonResult(final);
            }
            catch (Exception ex)
            {
                return new JsonResult(new List<object>());
            }
        }

        public IActionResult OnPostAddToCart(int id, int quantity)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return RedirectToPage();

            var sessionData = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(sessionData) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(sessionData)!;

            var existing = cart.FirstOrDefault(c => c.ProductId == id);
            if (existing != null) existing.Quantity += quantity;
            else cart.Add(new CartItem { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = quantity });

            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetSearchAsync(string searchString, string sortOrder, string category, decimal? minPrice, decimal? maxPrice, int pageNumber = 1)
        {
            PageNumber = pageNumber;
            await LoadProducts(searchString, sortOrder, category, minPrice, maxPrice);
            return Partial("_ProductListPartial", this);
        }

        private async Task LoadProducts(string searchString, string sortOrder, string? category, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString)) query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{searchString.ToLower()}%"));
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(p => p.Category == category);
            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

            query = sortOrder switch
            {
                "NameAsc" => query.OrderBy(p => p.Name),
                "NameDesc" => query.OrderByDescending(p => p.Name),
                "PriceAsc" => query.OrderBy(p => p.Price),
                "PriceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UserFavoriteIds = await _context.Favorites
                    .Where(f => f.UserId == user.Id)
                    .Select(f => f.ProductId)
                    .ToListAsync();
            }

            var total = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(total / (double)PageSize);
            Products = await query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync();
            Categories = await _context.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
        }
    }
}