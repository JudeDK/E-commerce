using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProiectWeb.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private const int PageSize = 10;

        private static readonly HashSet<string> ValidSorts = new(StringComparer.OrdinalIgnoreCase)
        {
            "newest", "oldest", "largest", "smallest"
        };

        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        public Dictionary<int, int> GlobalOrderNumbers { get; set; } = new();

        public Dictionary<int, decimal> OrderTotals { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "newest";

        public int TotalPages { get; set; }

        public int TotalOrders { get; set; }

        public async Task OnGetAsync()
        {
            if (!ValidSorts.Contains(Sort))
                Sort = "newest";

            var allByDate = await _context.Orders
                .OrderBy(o => o.OrderDate)
                .ThenBy(o => o.Id)
                .Select(o => o.Id)
                .ToListAsync();

            for (int i = 0; i < allByDate.Count; i++)
                GlobalOrderNumbers[allByDate[i]] = i + 1;

            var query = _context.Orders.AsQueryable();

            TotalOrders = await query.CountAsync();
            TotalPages = TotalOrders == 0 ? 1 : (int)System.Math.Ceiling(TotalOrders / (double)PageSize);
            PageNumber = System.Math.Clamp(PageNumber, 1, TotalPages);

            IQueryable<Order> ordered = Sort.ToLowerInvariant() switch
            {
                "oldest" => query.OrderBy(o => o.OrderDate).ThenBy(o => o.Id),
                "largest" => query
                    .OrderByDescending(o => o.Items.Sum(i => (decimal)i.Quantity * (i.Product != null ? i.Product.Price : 0)))
                    .ThenByDescending(o => o.OrderDate),
                "smallest" => query
                    .OrderBy(o => o.Items.Sum(i => (decimal)i.Quantity * (i.Product != null ? i.Product.Price : 0)))
                    .ThenByDescending(o => o.OrderDate),
                _ => query.OrderByDescending(o => o.OrderDate).ThenBy(o => o.Id),
            };

            Orders = await ordered
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            foreach (var order in Orders)
            {
                OrderTotals[order.Id] = order.Items.Sum(i =>
                    i.Quantity * (i.Product?.Price ?? 0));
            }
        }

        public decimal GetOrderTotal(int orderId) =>
            OrderTotals.GetValueOrDefault(orderId, 0);
    }
}
