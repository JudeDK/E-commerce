namespace ProiectWeb.API;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public static class Chatbox
{
    public static void MapChatApi(this WebApplication app)
    {
        app.MapPost("/api/chat", async (HttpRequest request, ApplicationDbContext db) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            var data = JsonSerializer.Deserialize<ChatRequest>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (string.IsNullOrWhiteSpace(data?.Question))
                return Results.Json(new { answer = "Mesaj invalid." });

            var question = data.Question.ToLower().Trim();

            // -------------------------------------------------------------------
            // 🔹 1. Detectare întrebare despre stoc (cu filtru inteligent și Regex)
            // -------------------------------------------------------------------
            string[] cuvinteStoc =
            {
    "stoc", "pe stoc", "disponibil", "mai aveti", "mai este", "mai sunt",
    "in stoc", "se mai poate comanda", "se mai gaseste", "disponibilitate"
};

            string[] cuvinteIntentie =
            {
    "ai", "aveti", "aveți", "gasesti", "găsești", "exista", "există", "se gaseste", "se găsește"
};

            string[] cuvinteBlacklist =
            {
    "reteta", "rețetă", "gatit", "găti", "mancare", "mâncare",
    "preparat", "bucatarie", "desert", "cina", "pranz", "gustare"
};

            if (!cuvinteBlacklist.Any(b => question.Contains(b)))
            {
                var produse = await db.Products.ToListAsync();

                foreach (var produs in produse)
                {
                    var nume = produs.Name.ToLower();

                    bool produsMentionat = Regex.IsMatch(question, $@"\b{Regex.Escape(nume)}\b", RegexOptions.IgnoreCase);
                    bool intrebareDespreStoc = cuvinteStoc.Any(c => question.Contains(c)) || cuvinteIntentie.Any(c => question.StartsWith(c));

                    // ✅ Dacă întrebarea e gen "ai banane?" → merge pe aceeași logică de stoc
                    if (produsMentionat && intrebareDespreStoc)
                    {
                        if (produs.Quantity > 0)
                            return Results.Json(new { answer = $"Da, produsul {produs.Name} este în stoc ({produs.Quantity} bucăți disponibile)." });
                        else
                            return Results.Json(new { answer = $"Momentan produsul {produs.Name} nu este pe stoc." });
                    }
                }

                // ✅ fallback: dacă s-a întrebat de o categorie în loc de produs
                var categorii = await db.Products
                    .Select(p => p.Category.ToLower())
                    .Distinct()
                    .ToListAsync();

                foreach (var categorie in categorii)
                {
                    if (Regex.IsMatch(question, $@"\b{Regex.Escape(categorie)}\b", RegexOptions.IgnoreCase)
                        && (cuvinteStoc.Any(c => question.Contains(c)) || cuvinteIntentie.Any(c => question.StartsWith(c))))
                    {
                        var produseCategorie = await db.Products
                            .Where(p => p.Category.ToLower() == categorie && p.Quantity > 0)
                            .ToListAsync();

                        if (produseCategorie.Count == 0)
                            return Results.Json(new { answer = $"Momentan nu avem produse disponibile în categoria {categorie}." });

                        var listaProduse = string.Join(", ", produseCategorie.Select(p => p.Name));
                        return Results.Json(new { answer = $"În categoria {categorie} avem următoarele produse pe stoc: {listaProduse}." });
                    }
                }

                // Dacă s-a întrebat despre produse, dar nu se găsește nimic
                if (cuvinteIntentie.Any(c => question.StartsWith(c)) || cuvinteStoc.Any(c => question.Contains(c)))
                {
                    return Results.Json(new { answer = "Nu am găsit produsul sau categoria menționată în stocul nostru." });
                }
            }


            // -------------------------------------------------------------------
            // 🔹 3. Întrebări generale despre categorii (ex: „Ce produse sunt la fructe?”)
            // -------------------------------------------------------------------
            string[] cuvinteCategorie =
            {
                "categorie", "categoriea", "categoriei", "la", "din categoria", "de la",
                "ce produse", "există ceva la", "aveti ceva la", "ai"
            };

            if (cuvinteCategorie.Any(c => question.Contains(c)))
            {
                var categorii = await db.Products
                   .Select(p => p.Category.ToLower())
                   .Distinct()
                   .ToListAsync();

                foreach (var categorie in categorii)
                {
                    if (Regex.IsMatch(question, $@"\b{Regex.Escape(categorie)}\b", RegexOptions.IgnoreCase))
                    {
                        var produseCategorie = await db.Products
                            .Where(p => p.Category.ToLower() == categorie)
                            .ToListAsync();

                        if (produseCategorie.Count == 0)
                            return Results.Json(new { answer = $"Momentan nu avem produse disponibile în categoria {categorie}." });

                        var listaProduse = string.Join(", ", produseCategorie.Select(p => p.Name));
                        return Results.Json(new { answer = $"În categoria {categorie} avem următoarele produse: {listaProduse}." });
                    }
                }

                return Results.Json(new { answer = "Poți te rog să specifici categoria? De exemplu: 'Ce produse aveți la băuturi' sau 'Aveți ceva la fructe'." });
            }

            // -------------------------------------------------------------------
            // 🔹 4. Dacă nu e întrebare de stoc/categorie — trimite la AI Groq
            // -------------------------------------------------------------------
            var configuration = app.Services.GetService<IConfiguration>();
            var apiKey = configuration?["Groq:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
                return Results.Json(new { answer = "Cheia API lipsește din configurare." });

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var content = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = @"Ești un asistent tehnic inteligent și realist al magazinului online.
Scopul tău este să ajuți clienții să rezolve probleme legate de produse, stocuri, conturi, comenzi și funcționarea site-ului.

💼 RESPONSABILITĂȚI PRINCIPALE:
- Oferi informații despre produse, stocuri și disponibilitate.
- Ghidezi utilizatorii care nu se pot conecta, nu pot crea cont sau au uitat parola.
- Explici pașii pentru comenzi, plăți, retururi și confirmări.
- Oferi ajutor la erori tehnice (pagini care nu se încarcă, coș blocat etc.).
- Poți recomanda pași practici, dar nu inventa informații.

⚠️ LIMITĂRI:
- Nu poți accesa direct conturile clienților, ci doar să explici cum să procedeze.
- Nu oferi răspunsuri la întrebări fără legătură cu magazinul (ex: rețete, glume, geografie, cultură generală, sfaturi de viață).
- Dacă întrebarea nu ține de magazin, răspunde exact cu:
„Îmi pare rău, pot răspunde doar la întrebări legate de produsele, comenzile sau funcționarea magazinului nostru.”

💬 STIL DE COMUNICARE:
- Vorbește natural, clar și politicos, dar fără să fii excesiv de formal.
- Fii empatic doar atunci când utilizatorul exprimă o problemă sau frustrare reală.
- Evită să spui expresii precum „îmi pare rău” sau „scuze” la fiecare răspuns — folosește-le doar când e justificat.
- Nu menționa termeni tehnici precum „bază de date”, „API” sau „sistem intern”.
- Nu spune niciodată că ești un AI sau ce model folosești. Ești „Asistentul magazinului online”."
                    },

                    new { role = "user", content = data.Question }
                }
            };

            var json = JsonSerializer.Serialize(content);
            var response = await httpClient.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var respBody = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(respBody);

                if (!doc.RootElement.TryGetProperty("choices", out var choices))
                {
                    if (doc.RootElement.TryGetProperty("error", out var err))
                    {
                        var msg = err.GetProperty("message").GetString();
                        return Results.Json(new { answer = $"Eroare API: {msg}" });
                    }
                    return Results.Json(new { answer = "A apărut o eroare neașteptată de la Groq." });
                }

                var answer = choices[0].GetProperty("message").GetProperty("content").GetString();
                return Results.Json(new { answer });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la parsare Groq: " + ex.Message);
                Console.WriteLine("Răspuns complet: " + respBody);
                return Results.Json(new { answer = "A apărut o eroare la procesarea răspunsului AI." });
            }
        });
    }

    private record ChatRequest(string Question);
}
