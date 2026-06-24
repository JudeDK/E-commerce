using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;

namespace ProiectWeb.Services;

/// <summary>
/// Reguli prescriptive pentru analiza AI (stagnante + favorite) — fără răspunsuri random.
/// </summary>
public class PrescriptivePricingService
{
    private readonly ApplicationDbContext _db;
    private const decimal MinMarginPercent = 15m;

    public PrescriptivePricingService(ApplicationDbContext db) => _db = db;

    public static decimal GetAcquisitionCost(Product p) =>
        p.AcquisitionCost > 0 ? p.AcquisitionCost : Math.Round(p.Price * 0.65m, 2);

    public static decimal MinAllowedPrice(Product p)
    {
        var cost = GetAcquisitionCost(p);
        return Math.Round(cost * (1 + MinMarginPercent / 100m), 2);
    }

    public async Task<string> BuildStagnantAnalysisAsync()
    {
        var now = DateTime.UtcNow;
        var produse = await _db.Products.AsNoTracking().ToListAsync();

        var salesDates = await _db.OrderItems.AsNoTracking()
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, Ultima = g.Max(x => x.Order.OrderDate) })
            .ToListAsync();
        var lastSale = salesDates.ToDictionary(x => x.ProductId, x => x.Ultima);

        var lines = new List<string>();
        foreach (var p in produse)
        {
            int zile;
            if (!lastSale.TryGetValue(p.Id, out var ultima))
                zile = int.MaxValue;
            else
                zile = (int)(now - ultima).TotalDays;

            if (zile < 30)
                continue;

            decimal reducere = zile >= 60 ? 0.10m : 0.05m;
            var pretNou = Math.Round(p.Price * (1 - reducere), 2);
            var pretMinim = MinAllowedPrice(p);

            if (pretNou < pretMinim)
                pretNou = pretMinim;

            var marja = p.Price > 0
                ? Math.Round((pretNou - GetAcquisitionCost(p)) / pretNou * 100, 1)
                : 0;

            var zileLabel = zile == int.MaxValue ? "fără vânzări" : $"{zile} zile";
            var motiv = zile >= 60
                ? $"stagnează de {zileLabel} (≥60 zile → reducere 10%)"
                : $"stagnează de {zileLabel} (≥30 zile → reducere 5%)";

            lines.Add(
                $"- **{p.Name}** ({zileLabel}): Sugerez reducere cu **{(int)(reducere * 100)}%**. " +
                $"Preț curent **{p.Price:0.00}** RON → preț nou **{pretNou:0.00}** RON, marjă **{marja}%**. " +
                $"Motiv: {motiv}.");
        }

        if (lines.Count == 0)
            return "Nu există produse care necesită lichidare (stagnare ≥30 zile).";

        return "**ANALIZĂ PRESCRIPTIVĂ — LICHIDARE STOC:**\n" + string.Join("\n", lines.Take(10));
    }

    public async Task<string> BuildFavoritesAnalysisAsync()
    {
        var produse = await _db.Products.AsNoTracking().ToListAsync();
        if (produse.Count == 0)
            return "Niciun produs în catalog.";

        var vanzari = await _db.OrderItems.AsNoTracking()
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToListAsync();
        var vanzariDict = vanzari.ToDictionary(x => x.ProductId, x => x.Total);

        var favorite = await _db.Favorites.AsNoTracking()
            .GroupBy(f => f.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToListAsync();
        var favDict = favorite.ToDictionary(x => x.ProductId, x => x.Count);

        var ranked = produse
            .Select(p => new
            {
                Product = p,
                Vanzari = vanzariDict.GetValueOrDefault(p.Id, 0),
                Favorite = favDict.GetValueOrDefault(p.Id, 0),
            })
            .OrderByDescending(x => x.Vanzari)
            .ToList();

        var top20Count = Math.Max(1, (int)Math.Ceiling(ranked.Count * 0.20));
        var top20Ids = ranked.Take(top20Count).Select(x => x.Product.Id).ToHashSet();
        var avgFav = favDict.Count > 0 ? favDict.Values.Average() : 0;

        var lines = new List<string>();
        foreach (var row in ranked.Where(x => top20Ids.Contains(x.Product.Id) && x.Favorite >= Math.Max(1, avgFav)))
        {
            var crestere = row.Favorite >= avgFav * 2 ? 0.10m : 0.05m;
            var pretNou = Math.Round(row.Product.Price * (1 + crestere), 2);
            if (pretNou > 500) pretNou = 500;

            lines.Add(
                $"- **{row.Product.Name}**: top vânzări ({row.Vanzari} buc.) + **{row.Favorite}** favorite. " +
                $"Sugerez scumpire **{(int)(crestere * 100)}%**: **{row.Product.Price:0.00}** → **{pretNou:0.00}** RON " +
                $"(test elasticitate preț, marjă crescută).");
        }

        if (lines.Count == 0)
        {
            var topFav = ranked.OrderByDescending(x => x.Favorite).Take(5);
            foreach (var row in topFav.Where(x => x.Favorite > 0))
            {
                lines.Add($"- **{row.Product.Name}**: {row.Favorite} salvări (cerere moderată, fără criterii top 20% vânzări).");
            }
            if (lines.Count == 0)
                return "Niciun produs la favorite.";
            return "**TOP FAVORITE:**\n" + string.Join("\n", lines);
        }

        return "**ANALIZĂ PRESCRIPTIVĂ — MAXIMIZARE MARJĂ (FAVORITE + TOP VÂNZĂRI):**\n" + string.Join("\n", lines.Take(8));
    }
}
