using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;

namespace ProiectWeb.Services
{
    /// <summary>
    /// Imagini produse în folderul ProiectWeb/Imagini — doar numele fișierului în DB (fără cale).
    /// </summary>
    public class ProductImageService
    {
        public const string NotFoundFileName = "Imagine negasita.jpg";
        public const string UrlPrefix = "/Imagini";

        private readonly string _imaginiDir;

        public ProductImageService(IWebHostEnvironment env)
        {
            _imaginiDir = ResolveImaginiDirectory(env);
        }

        public string ImaginiDirectory => _imaginiDir;

        public static string ResolveImaginiDirectory(IWebHostEnvironment env)
        {
            foreach (var candidate in new[]
            {
                Path.Combine(env.ContentRootPath, "..", "Imagini"),
                Path.Combine(env.ContentRootPath, "Imagini"),
            })
            {
                var full = Path.GetFullPath(candidate);
                if (Directory.Exists(full))
                    return full;
            }

            return Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "Imagini"));
        }

        public string GetFileName(Product product) =>
            ResolveFileName(product.Name, product.Imagine);

        public string GetUrl(Product? product)
        {
            if (product == null)
                return GetUrlFromFileName(NotFoundFileName);
            return GetUrlFromFileName(GetFileName(product));
        }

        public string GetUrlByName(string productName, string? storedImagine = null) =>
            GetUrlFromFileName(ResolveFileName(productName, storedImagine));

        public string GetUrlFromFileName(string? fileName)
        {
            var file = string.IsNullOrWhiteSpace(fileName) || !FileExists(fileName)
                ? NotFoundFileName
                : fileName;
            return $"{UrlPrefix}/{Uri.EscapeDataString(file)}";
        }

        /// <summary>Rezolvă fișierul după numele produsului (ex: „Telefon X” → „Telefon X.jpg”).</summary>
        public string ResolveFileName(string productName, string? currentImagine = null)
        {
            if (!string.IsNullOrWhiteSpace(currentImagine)
                && currentImagine != NotFoundFileName
                && FileExists(currentImagine))
                return currentImagine;

            var byName = SanitizeFileName(productName);
            if (FileExists(byName))
                return byName;

            if (!Directory.Exists(_imaginiDir))
                return NotFoundFileName;

            var normalizedProduct = NormalizeForFileMatch(productName);
            var match = Directory.EnumerateFiles(_imaginiDir, "*.jpg", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path =>
                    string.Equals(
                        NormalizeForFileMatch(Path.GetFileNameWithoutExtension(path)),
                        normalizedProduct,
                        StringComparison.OrdinalIgnoreCase));

            return match != null ? Path.GetFileName(match) : NotFoundFileName;
        }

        /// <summary>Elimină caractere interzise în nume fișier (ex. ") — imaginile sunt salvate fără ele.</summary>
        public static string NormalizeForFileMatch(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var invalid = Path.GetInvalidFileNameChars();
            var chars = value.Where(c => !invalid.Contains(c)).ToArray();
            return new string(chars).Trim();
        }

        public static string SanitizeFileName(string productName)
        {
            var cleaned = NormalizeForFileMatch(productName);
            if (string.IsNullOrEmpty(cleaned))
                cleaned = "produs";
            return cleaned + ".jpg";
        }

        public string GetUploadPath(string productName) =>
            Path.Combine(_imaginiDir, SanitizeFileName(productName));

        public bool FileExists(string fileName) =>
            File.Exists(Path.Combine(_imaginiDir, fileName));

        public async Task<int> SyncProductImagesAsync(ApplicationDbContext context)
        {
            if (!Directory.Exists(_imaginiDir))
                Directory.CreateDirectory(_imaginiDir);

            var products = await context.Products.ToListAsync();
            var updated = 0;

            foreach (var product in products)
            {
                var resolved = ResolveFileName(product.Name, product.Imagine);
                if (product.Imagine != resolved)
                {
                    product.Imagine = resolved;
                    updated++;
                }
            }

            if (updated > 0)
                await context.SaveChangesAsync();

            return updated;
        }
    }
}
