using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProiectWeb.Data;
using ProiectWeb.Models;
using Stripe;
using Stripe.Checkout;

namespace ProiectWeb.Pages.Client.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Introducem cheia ta de test aici
        private const string StripeSecretKey = "sk_test_51SjaGrERe0IKlAF8yNROdPGl8ZahcC68jdwQ9rAIN4whsFItnQeVRThJs2Gx3sCHXutE398CiRgJaIsP8Hm64WqM00eCET99t1";

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
            if (string.IsNullOrEmpty(sessionData)) return RedirectToPage();

            var cart = JsonConvert.DeserializeObject<List<CartItem>>(sessionData)!;
            cart.RemoveAll(p => p.ProductId == id);
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionData)) return RedirectToPage();

            var cart = JsonConvert.DeserializeObject<List<CartItem>>(sessionData)!;
            if (!cart.Any()) return RedirectToPage();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            // 1. Validare Stoc înainte de plată
            var productIds = cart.Select(c => c.ProductId).Distinct().ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            foreach (var item in cart)
            {
                if (!products.TryGetValue(item.ProductId, out var prod) || prod.Quantity < item.Quantity)
                {
                    TempData["Message"] = $"Stoc insuficient pentru {prod?.Name ?? "un produs"}.";
                    return RedirectToPage();
                }
            }

            // 2. Configurare Stripe
            StripeConfiguration.ApiKey = StripeSecretKey;

            var domain = $"{Request.Scheme}://{Request.Host}";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = cart.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // Stripe folosește bani (RON * 100)
                        Currency = "ron",
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.ProductName },
                    },
                    Quantity = item.Quantity,
                }).ToList(),
                Mode = "payment",
                // Redirecționăm către noua pagină de succes
                SuccessUrl = domain + "/Client/Cart/OrderSuccess?sessionId={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/Client/Cart/Index",
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            // 3. Creăm comanda cu status Neplătit și salvăm StripeSessionId
            var order = new ProiectWeb.Models.Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                IsPaid = false,
                StripeSessionId = session.Id,
                Items = cart.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 4. Trimitem utilizatorul pe pagina de plată Stripe
            return Redirect(session.Url);
        }
    }
}