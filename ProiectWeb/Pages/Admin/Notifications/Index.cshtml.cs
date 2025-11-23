using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ProiectWeb.Pages.Admin.Notifications
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Notification> Notifications { get; set; }

        public async Task OnGetAsync()
        {
            Notifications = await _context.Notifications
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();
        }
    }
}
