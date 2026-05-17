using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ProiectWeb.Data;
using ProiectWeb.Models;

namespace ProiectWeb.Pages.Client
{
    public class StatisticiModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticiModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string DateGraficJson { get; set; } = "[]";
        public string DateCategoriiJson { get; set; } = "[]";
        public decimal TotalLunaCurenta { get; set; }
        public decimal DiferentaProcentuala { get; set; }

        public async Task<IActionResult> OnGetAsync(int luni = 6)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var dataLimita = DateTime.Now.AddMonths(-luni);

            // Includem Items si Product pentru a accesa pretul si categoria
            var comenzi = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == user.Id && o.OrderDate >= dataLimita && o.IsPaid)
                .ToListAsync();

            if (comenzi == null || !comenzi.Any()) return Page();

            // 1. Evoluție Lunară (Line Chart)
            var dateGrupate = comenzi
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new {
                    Eticheta = $"{g.Key.Month}/{g.Key.Year}",
                    // Calculam pretul din Product.Price deoarece OrderItem nu are Price
                    Total = (double)g.Sum(o => o.Items.Sum(i => i.Product.Price * i.Quantity))
                })
                .OrderBy(x => x.Eticheta)
                .ToList();

            DateGraficJson = JsonSerializer.Serialize(dateGrupate);

            // 2. Distribuție pe Categorii (Doughnut Chart)
            var dateCategorii = comenzi
                .SelectMany(o => o.Items)
                .Where(oi => oi.Product != null)
                .GroupBy(oi => oi.Product.Category)
                .Select(g => new {
                    Categorie = g.Key,
                    Suma = (double)g.Sum(x => x.Product.Price * x.Quantity)
                }).ToList();

            DateCategoriiJson = JsonSerializer.Serialize(dateCategorii);

            // 3. Calcule Rezumat Card
            var ultimaLunaVal = dateGrupate.LastOrDefault()?.Total ?? 0;
            TotalLunaCurenta = (decimal)ultimaLunaVal;

            var totalLunaTrecuta = dateGrupate.Count >= 2 ? dateGrupate[^2].Total : 0;

            if (totalLunaTrecuta > 0)
                DiferentaProcentuala = (decimal)(((ultimaLunaVal - totalLunaTrecuta) / totalLunaTrecuta) * 100);

            return Page();
        }
    }
}