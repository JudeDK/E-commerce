using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProiectWeb.Data;
using ProiectWeb.Models;

namespace ProiectWeb.Pages.Client.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal Total => CartItems.Sum(i => i.Total);

        public void OnGet()
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(sessionData))
                CartItems = JsonConvert.DeserializeObject<List<CartItem>>(sessionData)!;
        }

        public IActionResult OnPostRemove(int id)
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionData))
                return RedirectToPage();

            var cart = JsonConvert.DeserializeObject<List<CartItem>>(sessionData)!;
            cart.RemoveAll(p => p.ProductId == id);
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionData))
                return RedirectToPage();

            var cart = JsonConvert.DeserializeObject<List<CartItem>>(sessionData)!;
            if (!cart.Any())
                return RedirectToPage();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login");

            // Încarcă produsele implicate într-un singur query
            var productIds = cart.Select(c => c.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Verifică existența produselor și stocul suficient
            foreach (var item in cart)
            {
                if (!products.TryGetValue(item.ProductId, out var prod))
                {
                    TempData["Message"] = $"Produsul cu ID {item.ProductId} nu mai există.";
                    return RedirectToPage();
                }

                if (item.Quantity < 1)
                {
                    TempData["Message"] = $"Cantitate invalidă pentru {prod.Name}.";
                    return RedirectToPage();
                }

                if (prod.Quantity < item.Quantity)
                {
                    TempData["Message"] = $"Stoc insuficient pentru {prod.Name}. Disponibil: {prod.Quantity}.";
                    return RedirectToPage();
                }
            }

            // Tranzacție: comandă + actualizare stocuri
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = user.Id,
                    OrderDate = DateTime.Now,
                    Items = cart.Select(c => new OrderItem
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity
                    }).ToList()
                };

                _context.Orders.Add(order);

                // Actualizează stocurile
                foreach (var item in cart)
                {
                    var product = products[item.ProductId];
                    product.Quantity -= item.Quantity;

                    // (opțional) barieră anti-negativ
                    if (product.Quantity < 0) product.Quantity = 0;
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // Golește coșul
                HttpContext.Session.Remove("Cart");

                TempData["Message"] = "Comanda ta a fost plasată cu succes!";
                return RedirectToPage("/Client/Products/Index");
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Message"] = "A apărut o eroare la procesarea comenzii. Te rugăm să încerci din nou.";
                return RedirectToPage();
            }
        }
    }
}
