using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using System.Text.Json;

namespace ProiectWeb.Services;

public class RecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly ProductImageService _images;
    private static JsonDocument? _cachedModel;
    private static DateTime _lastFileWrite = DateTime.MinValue;
    private static Dictionary<int, int>? _indexToProductId;
    private static readonly object Lock = new();

    private const double ContentWeight = 0.40;
    private const double CoOccurrenceWeight = 0.45;
    private const double CrossCategoryBoost = 0.20;
    private const int RecommendationCount = 3;

    public RecommendationService(ApplicationDbContext context, ProductImageService images)
    {
        _context = context;
        _images = images;
    }

    private void EnsureModelLoaded()
    {
        var jsonPath = ResolveModelPath();
        if (!File.Exists(jsonPath)) return;

        var writeTime = File.GetLastWriteTimeUtc(jsonPath);
        lock (Lock)
        {
            if (_cachedModel != null && writeTime <= _lastFileWrite) return;

            _cachedModel?.Dispose();
            _cachedModel = null;
            _indexToProductId = null;

            using var fs = File.OpenRead(jsonPath);
            _cachedModel = JsonDocument.Parse(fs);
            var productToIndex = _cachedModel.RootElement.GetProperty("product_id_to_index");
            _indexToProductId = productToIndex.EnumerateObject()
                .ToDictionary(x => x.Value.GetInt32(), x => int.Parse(x.Name));
            _lastFileWrite = writeTime;
        }
    }

    private static string ResolveModelPath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var path = Path.Combine(baseDir, "hybrid_model.json");
        if (File.Exists(path)) return path;
        return Path.Combine(Directory.GetCurrentDirectory(), "hybrid_model.json");
    }

    public async Task<List<object>> GetHybridRecommendationsAsync(int productId)
    {
        EnsureModelLoaded();

        var current = await _context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);
        if (current == null) return new List<object>();

        var categoryByProductId = await _context.Products.AsNoTracking()
            .ToDictionaryAsync(p => p.Id, p => p.Category);

        var hybridIds = TryGetHybridProductIds(productId, current.Category, categoryByProductId);
        if (hybridIds.Count > 0)
            return await LoadProductsByIdsAsync(hybridIds);

        var boughtTogether = await GetBoughtTogetherAsync(productId, current.Category);
        if (boughtTogether.Count > 0)
            return await LoadProductsByIdsAsync(boughtTogether);

        var crossPopular = await GetPopularCrossCategoryAsync(productId, current.Category);
        if (crossPopular.Count > 0)
            return await LoadProductsByIdsAsync(crossPopular);

        return (await _context.Products.AsNoTracking()
            .Where(p => p.Category != current.Category && p.Id != productId && p.Quantity > 0)
            .OrderBy(p => p.Name)
            .Take(RecommendationCount)
            .ToListAsync())
            .Select(MapProduct)
            .Cast<object>()
            .ToList();
    }

    private List<int> TryGetHybridProductIds(
        int productId,
        string currentCategory,
        Dictionary<int, string> categoryByProductId)
    {
        if (_cachedModel == null || _indexToProductId == null) return new List<int>();

        var root = _cachedModel.RootElement;
        var productToIndex = root.GetProperty("product_id_to_index");
        if (!productToIndex.TryGetProperty(productId.ToString(), out var indexElement))
            return new List<int>();

        int targetIdx = indexElement.GetInt32();
        var contentScores = root.GetProperty("content_similarity")[targetIdx];
        var coScores = root.GetProperty("conditional_popularity")[targetIdx];
        int count = contentScores.GetArrayLength();

        var scored = new List<(int index, double score)>();
        for (int i = 0; i < count; i++)
        {
            if (i == targetIdx) continue;

            double content = contentScores[i].GetDouble();
            double co = coScores[i].GetDouble();
            double score = (content * ContentWeight) + (co * CoOccurrenceWeight);

            if (_indexToProductId.TryGetValue(i, out var pid)
                && categoryByProductId.TryGetValue(pid, out var cat)
                && !string.Equals(cat, currentCategory, StringComparison.OrdinalIgnoreCase))
            {
                score += CrossCategoryBoost;
                if (co > 0.01) score += 0.10;
            }

            if (score > 0.005)
                scored.Add((i, score));
        }

        return scored
            .OrderByDescending(x => x.score)
            .Take(RecommendationCount * 2)
            .Select(x => _indexToProductId[x.index])
            .Distinct()
            .Take(RecommendationCount)
            .ToList();
    }

    private async Task<List<int>> GetBoughtTogetherAsync(int productId, string currentCategory)
    {
        var orderIds = await _context.OrderItems
            .Where(oi => oi.ProductId == productId)
            .Select(oi => oi.OrderId)
            .Distinct()
            .ToListAsync();

        if (orderIds.Count == 0) return new List<int>();

        var pairs = await _context.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId) && oi.ProductId != productId)
            .Include(oi => oi.Product)
            .ToListAsync();

        return pairs
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Count = g.Count(),
                IsCrossCategory = g.First().Product.Category != currentCategory
            })
            .OrderByDescending(x => x.IsCrossCategory)
            .ThenByDescending(x => x.Count)
            .Take(RecommendationCount)
            .Select(x => x.ProductId)
            .ToList();
    }

    private async Task<List<int>> GetPopularCrossCategoryAsync(int productId, string currentCategory)
    {
        return await _context.OrderItems
            .Where(oi => oi.ProductId != productId && oi.Product.Category != currentCategory)
            .GroupBy(oi => oi.ProductId)
            .OrderByDescending(g => g.Sum(x => x.Quantity))
            .Take(RecommendationCount)
            .Select(g => g.Key)
            .ToListAsync();
    }

    private async Task<List<object>> LoadProductsByIdsAsync(List<int> ids)
    {
        var dbProducts = await _context.Products.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        return ids
            .Select(id => dbProducts.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .Select(p => MapProduct(p!))
            .Cast<object>()
            .ToList();
    }

    private object MapProduct(ProiectWeb.Models.Product p) => new
    {
        p.Id,
        p.Name,
        p.Price,
        p.Imagine,
        p.Description,
        ImageUrl = _images.GetUrl(p)
    };
}
