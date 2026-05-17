using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System.Text.Json;

namespace ProiectWeb.Services
{
    public class RecommendationService
    {
        private readonly ApplicationDbContext _context;
        private static JsonDocument? _cachedModel = null;
        private static DateTime _lastFileUpdate = DateTime.MinValue;
        private static readonly object _lock = new object();

       
        private static Dictionary<int, int>? _indexToProductId = null;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task LoadModelAsync()
        {
            
            if (_cachedModel != null) return;

            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hybrid_model.json");
            if (!File.Exists(jsonPath))
                jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "hybrid_model.json");

            if (File.Exists(jsonPath))
            {
                lock (_lock)
                {
                   
                    if (_cachedModel != null) return;

                    using (FileStream fs = File.OpenRead(jsonPath))
                    {
                     
                        _cachedModel = JsonDocument.Parse(fs);

                        var productToIndex = _cachedModel.RootElement.GetProperty("product_id_to_index");
                        _indexToProductId = productToIndex.EnumerateObject()
                            .ToDictionary(x => x.Value.GetInt32(), x => int.Parse(x.Name));
                    }
                  
                }
            }
        }

   
        public async Task<List<object>> GetHybridRecommendationsAsync(int productId)
        {
            await LoadModelAsync();

            if (_cachedModel != null && _indexToProductId != null)
            {
                var root = _cachedModel.RootElement;
                var productToIndex = root.GetProperty("product_id_to_index");
                string pIdStr = productId.ToString();

                if (productToIndex.TryGetProperty(pIdStr, out var indexElement))
                {
                    int targetIdx = indexElement.GetInt32();
                    var contentScores = root.GetProperty("content_similarity")[targetIdx];
                    var coScores = root.GetProperty("conditional_popularity")[targetIdx];

                    int count = contentScores.GetArrayLength();
                    var combinedResults = new List<(int index, double score)>();

                    for (int i = 0; i < count; i++)
                    {
                        if (i == targetIdx) continue;

                        double score = (contentScores[i].GetDouble() * 0.7) + (coScores[i].GetDouble() * 0.3);
                        if (score > 0.01) combinedResults.Add((i, score));
                    }

                    if (combinedResults.Any())
                    {
                   
                        var top3Indices = combinedResults
                            .OrderByDescending(x => x.score)
                            .Take(3)
                            .Select(x => x.index)
                            .ToList();

                        var top3Ids = top3Indices.Select(idx => _indexToProductId[idx]).ToList();

                      
                        var dbProducts = await _context.Products
                            .AsNoTracking()
                            .Where(p => top3Ids.Contains(p.Id))
                            .Select(p => new { p.Id, p.Name, p.Price, p.Imagine })
                            .ToListAsync();

                        return top3Ids
                            .Select(id => (object)dbProducts.FirstOrDefault(p => p.Id == id)!)
                            .Where(p => p != null)
                            .ToList();
                    }
                }
            }

            var currentProd = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (currentProd == null) return new List<object>();

            return await _context.Products
                .AsNoTracking()
                .Where(p => p.Category == currentProd.Category && p.Id != productId)
                .Take(3)
                .Select(p => (object)new { p.Id, p.Name, p.Price, p.Imagine })
                .ToListAsync();
        }

       
    }
}