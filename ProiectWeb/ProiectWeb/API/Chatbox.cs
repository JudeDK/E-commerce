namespace ProiectWeb.API;

using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using ProiectWeb.Models;
using System.Text;
using System.Text.Json;

using ProiectWeb.Services;

public static class Chatbox
{
    private const int CriticalStockThreshold = 15;

    public static void MapChatApi(this WebApplication app)
    {
        var aiBaseUrl = app.Configuration["AiServer:BaseUrl"] ?? "http://127.0.0.1:8001";

        app.MapPost("/api/chat", async (HttpContext httpContext, ApplicationDbContext db, PrescriptivePricingService prescriptive) =>
        {
            var question = "";
            try
            {
                using var reader = new StreamReader(httpContext.Request.Body);
                var body = await reader.ReadToEndAsync();
                var data = JsonSerializer.Deserialize<ChatRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (string.IsNullOrEmpty(data?.Question))
                    return Results.Json(new { answer = "Mesaj gol." });

                question = data.Question.Trim();
                bool isAdmin = httpContext.User.IsInRole("Admin");

                if (question.StartsWith("admin_", StringComparison.OrdinalIgnoreCase) && !isAdmin)
                {
                    return Results.Json(new
                    {
                        answer = "Comenzile Business Intelligence necesită cont Admin. Autentifică-te și reîncearcă.",
                    }, statusCode: StatusCodes.Status403Forbidden);
                }

                // Comenzi admin deterministe — răspuns direct din PostgreSQL (fără Python)
                if (isAdmin)
                {
                    switch (question)
                    {
                        case "admin_stoc_critic":
                            return Results.Json(new { answer = await BuildCriticalStockAnswerAsync(db) });

                        case "admin_stoc_stagnant_30":
                            return Results.Json(new { answer = await prescriptive.BuildStagnantAnalysisAsync() });

                        case "admin_analiza_favorite":
                            return Results.Json(new { answer = await prescriptive.BuildFavoritesAnalysisAsync() });

                        case "admin_propunere_comenzi":
                            return Results.Json(await BuildOrderProposalResponseAsync(db, aiBaseUrl));
                    }
                }

                object dbContextData;
                if (isAdmin)
                {
                    dbContextData = await BuildAdminPythonContextAsync(db, question);
                    Console.WriteLine($"[CHAT] Admin → Python: {((System.Collections.ICollection)dbContextData).Count} produse ({question})");
                }
                else
                {
                    dbContextData = BuildClientSupportContext(httpContext);
                }

                var payload = new Dictionary<string, object?>
                {
                    ["question"] = question,
                    ["session_id"] = httpContext.User.Identity?.Name ?? "guest",
                    ["role"] = isAdmin ? "Admin" : "User",
                    ["db_context"] = dbContextData,
                };

                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
                var jsonPayload = JsonSerializer.Serialize(payload, jsonOptions);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{aiBaseUrl.TrimEnd('/')}/chat", content);

                if (response.IsSuccessStatusCode)
                {
                    var respBody = await response.Content.ReadAsStringAsync();
                    return Results.Content(respBody, "application/json");
                }

                return Results.Json(new { answer = "Eroare la procesarea datelor în serverul AI." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHAT ERR]: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[CHAT ERR] Inner: {ex.InnerException.Message}");

                var hint = "Eroare server. Verifică logurile din terminal.";

                return Results.Json(new { answer = hint });
            }
        });
    }

    private static async Task<string> BuildCriticalStockAnswerAsync(ApplicationDbContext db)
    {
        var critice = await db.Products
            .AsNoTracking()
            .Where(p => p.Quantity <= CriticalStockThreshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Name)
            .Select(p => new { p.Name, p.Quantity })
            .ToListAsync();

        if (critice.Count == 0)
            return $"Nu există produse cu stoc critic (≤{CriticalStockThreshold} buc.).";

        var lines = critice.Select(p => $"- {p.Name}: {p.Quantity} buc.");
        return "**STOC CRITIC:**\n" + string.Join("\n", lines);
    }

    private static async Task<object> BuildOrderProposalResponseAsync(ApplicationDbContext db, string aiBaseUrl)
    {
        const int lowStockLimit = 20;

        var candidat = await db.Products
            .AsNoTracking()
            .Where(p => p.Quantity < lowStockLimit)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Quantity })
            .FirstOrDefaultAsync();

        if (candidat == null)
            return new { answer = "Momentan nu există propuneri de comenzi." };

        var dataLimita = DateTime.UtcNow.AddDays(-14);
        var istoricRaw = await db.OrderItems
            .AsNoTracking()
            .Where(oi => oi.ProductId == candidat.Id && oi.Order.OrderDate >= dataLimita)
            .Select(oi => new { oi.Order.OrderDate, oi.Quantity })
            .ToListAsync();

        var istoric = istoricRaw
            .GroupBy(x => x.OrderDate.Date)
            .Select(g => new { data = g.Key.ToString("yyyy-MM-dd"), cantitate = g.Sum(x => x.Quantity) })
            .OrderBy(x => x.data)
            .ToList<object>();

        var totalVanzari = istoricRaw.Sum(x => x.Quantity);
        var cantitate = await TryProphetQuantityAsync(aiBaseUrl, candidat.Name, candidat.Quantity, istoric)
            ?? CalculeazaCantitateReaprovizionare(candidat.Quantity, totalVanzari, istoric.Count);

        return new
        {
            answer = $"**Propunere:** {candidat.Name} — stoc actual **{candidat.Quantity}** bucăți",
            action = new
            {
                type = "execute_order",
                product = candidat.Name,
                quantity = cantitate,
            },
        };
    }

    /// <summary>
    /// Aceeași logică ca stock_prediction.py (fără Prophet ML).
    /// </summary>
    private static int CalculeazaCantitateReaprovizionare(int stocCurent, int totalVanzari, int zileCuVanzari)
    {
        const int minOrderQty = 10;
        const int leadTimeZile = 3;
        const int zileAcoperire = 30;

        double vanzariZilnice = 2.0;
        if (zileCuVanzari > 0)
        {
            vanzariZilnice = zileCuVanzari < 5
                ? Math.Max(0.1, totalVanzari / (double)zileCuVanzari)
                : Math.Max(0.1, totalVanzari / 14.0);
        }

        var stocSiguranta = (int)Math.Ceiling(vanzariZilnice * leadTimeZile * 0.2);
        var stocTinta = (int)Math.Ceiling(vanzariZilnice * zileAcoperire + stocSiguranta);
        var cantitate = Math.Max(0, stocTinta - stocCurent);

        if (stocCurent <= CriticalStockThreshold)
        {
            var minimCritic = stocCurent < 20 ? 20 - stocCurent : minOrderQty;
            cantitate = Math.Max(cantitate, Math.Max(minOrderQty, minimCritic));
        }
        else if (cantitate <= 0)
        {
            var rop = (int)Math.Ceiling(vanzariZilnice * leadTimeZile + stocSiguranta);
            cantitate = Math.Max(minOrderQty, rop - stocCurent);
        }

        return Math.Max(minOrderQty, cantitate);
    }

    private static async Task<int?> TryProphetQuantityAsync(
        string aiBaseUrl,
        string productName,
        int stoc,
        List<object> istoric)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
            var baseUrl = aiBaseUrl.TrimEnd('/');

            using var healthResp = await client.GetAsync($"{baseUrl}/health");
            if (!healthResp.IsSuccessStatusCode)
                return null;

            var payload = new Dictionary<string, object?>
            {
                ["question"] = "admin_propunere_comenzi",
                ["session_id"] = "dotnet-prophet",
                ["role"] = "Admin",
                ["db_context"] = new[]
                {
                    new Dictionary<string, object?>
                    {
                        ["nume"] = productName,
                        ["stoc"] = stoc,
                        ["istoric_vanzari"] = istoric,
                    },
                },
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = null });
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await client.PostAsync($"{baseUrl}/chat", content);
            if (!resp.IsSuccessStatusCode)
                return null;

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("action", out var action)
                && action.TryGetProperty("quantity", out var qtyEl)
                && qtyEl.TryGetInt32(out var qty))
            {
                return qty;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CHAT] Prophet indisponibil, fallback local: {ex.Message}");
        }

        return null;
    }

    private static async Task<List<object>> BuildAdminPythonContextAsync(ApplicationDbContext db, string question)
    {
        var dataLimitaIstoric = DateTime.UtcNow.AddDays(-14);
        IQueryable<Product> query = db.Products.AsNoTracking();

        if (question == "admin_propunere_comenzi")
            query = query.OrderBy(p => p.Quantity).Take(30);
        else
            query = query.Take(50);

        var products = await query
            .Select(p => new { p.Id, p.Name, p.Quantity })
            .ToListAsync();

        var productIds = products.Select(p => p.Id).ToList();
        if (productIds.Count == 0)
            return new List<object>();

        var salesRaw = await db.OrderItems
            .AsNoTracking()
            .Where(oi => productIds.Contains(oi.ProductId) && oi.Order.OrderDate >= dataLimitaIstoric)
            .Select(oi => new { oi.ProductId, OrderDate = oi.Order.OrderDate, oi.Quantity })
            .ToListAsync();

        var salesLookup = salesRaw
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .GroupBy(x => x.OrderDate.Date)
                    .Select(d => new { data = d.Key.ToString("yyyy-MM-dd"), cantitate = d.Sum(x => x.Quantity) })
                    .OrderBy(d => d.data)
                    .ToList()
            );

        var result = new List<object>();
        foreach (var p in products)
        {
            var istoric = salesLookup.TryGetValue(p.Id, out var days) && days.Count > 0
                ? (object)days
                : Array.Empty<object>();
            result.Add(new { nume = p.Name, stoc = p.Quantity, istoric_vanzari = istoric });
        }
        return result;
    }

    private static object BuildClientSupportContext(HttpContext httpContext)
    {
        const string contactEmail = "raresmarian3344@gmail.com";
        var isAuth = httpContext.User.Identity?.IsAuthenticated == true;
        var isClient = httpContext.User.IsInRole("User");

        var cartItems = 0;
        var cartJson = httpContext.Session.GetString("Cart");
        if (!string.IsNullOrEmpty(cartJson))
        {
            try
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                cartItems = cart?.Sum(c => c.Quantity) ?? 0;
            }
            catch { /* sesiune coș invalidă */ }
        }

        return new
        {
            utilizator_autentificat = isAuth,
            rol_client = isClient,
            produse_in_cos_acum = cartItems,
            email_contact = contactEmail,
            cum_functioneaza_cosul = new[]
            {
                "Coșul se salvează în sesiunea browserului; e nevoie de cont Client (rol User) conectat.",
                "Adăugare produs: Magazin → click pe poza produsului → în modal „Adaugă în coș”.",
                "Poți verifica articolele în meniul Coș; plata e prin Stripe pe pagina Coș.",
                "Dacă butonul nu reacționează: reîncarcă pagina (Ctrl+F5), dezactivează temporar ad-blocker, alt browser.",
                "Produsele fără stoc nu pot fi adăugate; mesajul ar trebui să apară după click.",
            },
            rezolvare_probleme_cos = new[]
            {
                "Confirmă că ești logat ca client (nu doar vizitator sau admin în altă fereastră).",
                "Încearcă din pagina Magazin (/Client/Products), nu din Favorite (acolo doar vezi detalii).",
                "După „Adaugă în coș”, deschide Coș din navbar — ar trebui să vezi produsul.",
                "Șterge cookie-urile site-ului doar dacă pașii de mai sus nu merg, apoi autentifică-te din nou.",
            },
        };
    }

    public record ChatRequest(string Question);
}
