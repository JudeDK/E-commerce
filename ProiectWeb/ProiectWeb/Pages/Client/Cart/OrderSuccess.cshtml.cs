using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using Stripe;
using Stripe.Checkout;
using ProiectWeb.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ProiectWeb.Pages.Client.Cart
{
    public class OrderSuccessModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private const string StripeSecretKey = "sk_test_51SjaGrERe0IKlAF8yNROdPGl8ZahcC68jdwQ9rAIN4whsFItnQeVRThJs2Gx3sCHXutE398CiRgJaIsP8Hm64WqM00eCET99t1";

        public OrderSuccessModel(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> OnGetAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return RedirectToPage("/Index");

            StripeConfiguration.ApiKey = StripeSecretKey;
            var service = new SessionService();
            Session session = await service.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);

                if (order != null && !order.IsPaid)
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        order.IsPaid = true;

                        foreach (var item in order.Items)
                        {
                            var product = await _context.Products.FindAsync(item.ProductId);
                            if (product != null)
                            {
                                var qtyBefore = product.Quantity;
                                product.Quantity -= item.Quantity;
                                if (product.Quantity < 0) product.Quantity = 0;

                                _context.Notifications.Add(new Notification
                                {
                                    Message = $"Stoc scăzut: «{product.Name}» {qtyBefore} → {product.Quantity} buc. (comandă #{order.Id}).",
                                    DateCreated = DateTime.UtcNow,
                                });
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        // ✅ FOLOSIM SERVICIUL TĂU DE EMAIL CARE FUNCȚIONEAZĂ
                        if (order.User != null && !string.IsNullOrEmpty(order.User.Email))
                        {
                            string subject = $"Confirmare Comandă #{order.Id} - Plată Reușită";
                            string body = BuildEmailBody(order);

                            // Trimitem asincron folosind serviciul tău injectat
                            await _emailSender.SendEmailAsync(order.User.Email, subject, body);
                        }

                        HttpContext.Session.Remove("Cart");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine("Eroare la procesarea comenzii: " + ex.Message);
                    }
                }
            }
            return Page();
        }

        private string BuildEmailBody(Models.Order order)
        {
            string tabelProduse = "";
            foreach (var item in order.Items)
            {
                if (item.Product == null) continue;
                var unit = item.Product.Price;
                var lineTotal = item.Quantity * unit;
                tabelProduse += $@"
                    <tr>
                        <td style='padding:10px; border-bottom:1px solid #eee;'>{item.Quantity} x {item.Product.Name}</td>
                        <td style='padding:10px; border-bottom:1px solid #eee; text-align:right;'>Preț Unitar: {unit:0.00} RON</td>
                        <td style='padding:10px; border-bottom:1px solid #eee; text-align:right;'>Total linie: {lineTotal:0.00} RON</td>
                    </tr>";
            }

            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; padding: 20px;'>
                    <div style='text-align:center; margin-bottom: 20px;'>
                        <h1 style='color: #28a745;'>Plată Reușită!</h1>
                        <p style='color: #555;'>Vă mulțumim pentru cumpărături. Comanda dumneavoastră a fost înregistrată.</p>
                    </div>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <thead>
                            <tr style='background-color: #f9f9f9;'>
                                <th style='padding:10px; text-align:left;'>Produs</th>
                                <th style='padding:10px; text-align:right;'>Preț unitar</th>
                                <th style='padding:10px; text-align:right;'>Total linie</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tabelProduse}
                        </tbody>
                    </table>
                    <div style='margin-top:20px; text-align:center;'>
                        <p style='font-size: 14px; color: #888;'>Aceasta este o confirmare automată pentru tranzacția Stripe.</p>
                    </div>
                </div>";
        }
    }
}