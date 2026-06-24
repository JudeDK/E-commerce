using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;

namespace ProiectWeb.Pages;

public class AboutModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AboutModel(ApplicationDbContext context) => _context = context;

    public List<string> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        Categories = await _context.Products
            .AsNoTracking()
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}
