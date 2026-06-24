using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProiectWeb.Pages.Client
{
    [Authorize]
    public class IstoricModel : PageModel
    {
        private const int PageSize = 10;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IstoricModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Order> Orders { get; set; } = new();

        public Dictionary<int, int> OrderDisplayNumbers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; set; }

        public int TotalOrders { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userOrderIds = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderBy(o => o.OrderDate)
                .ThenBy(o => o.Id)
                .Select(o => o.Id)
                .ToListAsync();

            for (int i = 0; i < userOrderIds.Count; i++)
                OrderDisplayNumbers[userOrderIds[i]] = i + 1;

            var query = _context.Orders.Where(o => o.UserId == user.Id);

            TotalOrders = await query.CountAsync();
            TotalPages = TotalOrders == 0 ? 1 : (int)System.Math.Ceiling(TotalOrders / (double)PageSize);
            PageNumber = System.Math.Clamp(PageNumber, 1, TotalPages);

            Orders = await query
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }
    }
}
