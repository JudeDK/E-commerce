using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;

namespace ProiectWeb.Pages.Admin.Notifications
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private const int PageSize = 10;

        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Notification> Notifications { get; set; } = new List<Notification>();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; set; }

        public int TotalNotifications { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Notifications.AsQueryable();

            TotalNotifications = await query.CountAsync();
            TotalPages = TotalNotifications == 0 ? 1 : (int)Math.Ceiling(TotalNotifications / (double)PageSize);
            PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

            Notifications = await query
                .OrderByDescending(n => n.DateCreated)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
