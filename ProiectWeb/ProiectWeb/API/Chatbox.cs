namespace ProiectWeb.API;

using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using System.Text;
using System.Text.Json;

public static class Chatbox
{
    public static void MapChatApi(this WebApplication app)
    {
        app.MapPost("/api/chat", async (HttpContext httpContext, ApplicationDbContext db) =>
        {
            try
            {
                using var reader = new StreamReader(httpContext.Request.Body);
                var body = await reader.ReadToEndAsync();
                var data = JsonSerializer.Deserialize<ChatRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (string.IsNullOrEmpty(data?.Question)) return Results.Json(new { answer = "Mesaj gol." });

                bool isAdmin = httpContext.User.IsInRole("Admin");
                object dbContextData = new List<object>();

                if (isAdmin)
                {
                    var query = db.Products.AsQueryable();
                    var dataLimitaIstoric = DateTime.UtcNow.AddDays(-14); // Luăm ultimele 14 zile pentru modelul AI

                    if (data.Question == "admin_stoc_critic")
                    {
                        query = query.Where(p => p.Quantity < 10);
                    }
                    else if (data.Question == "admin_analiza_favorite")
                    {
                        query = query.Where(p => db.Favorites.Any(f => f.ProductId == p.Id));
                    }
                    else if (data.Question == "admin_stoc_stagnant_30")
                    {
                        var dataLimita = DateTime.UtcNow.AddDays(-30);
                        query = query.Where(p => !db.OrderItems.Any(oi => oi.ProductId == p.Id && oi.Order.OrderDate >= dataLimita));
                    }
                    else if (data.Question == "admin_propunere_comenzi")
                    {
                        // 1. Ordonăm crescător după stoc (produsele cu cantitatea cea mai mică ajung primele)
                        // 2. Selectăm doar primele 30 pentru a nu bloca serverul AI cu un JSON prea mare
                        query = query.OrderBy(p => p.Quantity).Take(30);
                    }
                    else
                    {
                        query = query.Take(50);
                    }

                    // datele din sql
                    var rawData = await query.Select(p => new {
                        Name = p.Name,
                        Quantity = p.Quantity,
                        NrFavorite = db.Favorites.Count(f => f.ProductId == p.Id),
                        NrVanzari = db.OrderItems.Where(oi => oi.ProductId == p.Id).Sum(oi => (int?)oi.Quantity) ?? 0,
                        UltimaVanzareDate = db.OrderItems
                            .Where(oi => oi.ProductId == p.Id)
                            .Max(oi => (DateTime?)oi.Order.OrderDate),
                        // istoric 14 zile
                        RecentOrders = db.OrderItems
                            .Where(oi => oi.ProductId == p.Id && oi.Order.OrderDate >= dataLimitaIstoric)
                            .Select(oi => new { oi.Order.OrderDate, oi.Quantity })
                            .ToList()
                    }).ToListAsync();

                    // datele pentru python
                    dbContextData = rawData.Select(p => new {
                        nume = p.Name,
                        stoc = p.Quantity,
                        nr_favorite = p.NrFavorite,
                        nr_vanzari = p.NrVanzari,
                        ultima_vanzare = p.UltimaVanzareDate?.ToString("yyyy-MM-dd") ?? "",

                        //vanzarile pe zile pentru meta 
                        istoric_vanzari = p.RecentOrders
                            .GroupBy(o => o.OrderDate.Date)
                            .Select(g => new {
                                data = g.Key.ToString("yyyy-MM-dd"),
                                cantitate = g.Sum(o => o.Quantity)
                            })
                            .OrderBy(x => x.data)
                            .ToList()
                    }).ToList();

                    Console.WriteLine($"[DEBUG]: Trimise {((IEnumerable<object>)dbContextData).Count()} produse filtrate către Python.");
                }

                var payload = new
                {
                    question = data.Question,
                    session_id = httpContext.User.Identity?.Name ?? "guest",
                    role = isAdmin ? "Admin" : "User",
                    db_context = dbContextData
                };

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Asigură-te că portul este corect. Aici ai 8001, anterior aveai 8000. 
                var response = await httpClient.PostAsync("http://127.0.0.1:8001/chat", content);

                if (response.IsSuccessStatusCode)
                {
                    var respBody = await response.Content.ReadAsStringAsync();
                    return Results.Content(respBody, "application/json");
                }

                return Results.Json(new { answer = "Eroare la procesarea datelor în serverul AI." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERR]: {ex.Message}");
                return Results.Json(new { answer = "Eroare conexiune server AI. Asigură-te că serverul Python rulează." });
            }
        });
    }
    public record ChatRequest(string Question);
}