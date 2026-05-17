using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ProiectWeb.API;
using ProiectWeb.Data;
using ProiectWeb.Models;
using ProiectWeb.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.'");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// =======================
// IDENTITY CONFIG
// =======================
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// =======================
// CUSTOM SERVICES
// =======================
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<TwoFactorService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddRazorPages();

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// =======================
// CREATE DEFAULT ADMIN
// =======================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = "admin@stock.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, Nickname = "Admin" };
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// =======================
// MIDDLEWARE PIPELINE
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// ENDPOINT EXECUTARE COMANDĂ (Nou adăugat)
// ==========================================
app.MapPost("/api/chat/execute-order", async (HttpContext httpContext, ApplicationDbContext db, IEmailSender emailSender, OrderPayload order) =>
{
    // Verificăm dacă cel care apasă este Admin
    if (!httpContext.User.IsInRole("Admin"))
        return Results.Json(new { success = false, message = "Acces interzis." }, statusCode: 403);

    var product = await db.Products.FirstOrDefaultAsync(p => p.Name == order.ProductName);
    if (product == null)
        return Results.Json(new { success = false, message = "Produsul nu a fost găsit." });

    // 1. Actualizăm stocul în baza de date
    product.Quantity += order.Quantity;
    await db.SaveChangesAsync();

    // 2. Trimitem E-mail către furnizor
    // Aici poți pune adresa ta de mail pentru test sau o adresă de furnizor
    string furnizorEmail = "raresmarian3344@gmail.com";
    string subiect = $"Comandă Nouă: {order.ProductName}";
    string corpMesaj = $@"
        <h3>Comandă de reaprovizionare</h3>
        <p>Vă rugăm să livrați următoarele produse:</p>
        <ul>
            <li><strong>Produs:</strong> {order.ProductName}</li>
            <li><strong>Cantitate:</strong> {order.Quantity} unități</li>
        </ul>
        <p>Data solicitării: {DateTime.Now:dd/MM/yyyy HH:mm}</p>";

    try
    {
        await emailSender.SendEmailAsync(furnizorEmail, subiect, corpMesaj);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[MAIL ERR]: {ex.Message}");
        // Continuăm chiar dacă mail-ul eșuează, stocul e deja actualizat
    }

    return Results.Json(new
    {
        success = true,
        message = $"Stoc actualizat! Noua cantitate: {product.Quantity}",
        newQuantity = product.Quantity
    });
});

// =======================
// AUTO-ASSIGN ROLE USER
// =======================
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated ?? false)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);
        if (user != null)
        {
            var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            var isUser = await userManager.IsInRoleAsync(user, "User");
            if (!isUser && !isAdmin)
            {
                await userManager.AddToRoleAsync(user, "User");
                await signInManager.RefreshSignInAsync(user);
            }
        }
    }
    await next();
});

app.MapRazorPages();
app.MapChatApi();

app.Run();

// Definirea obiectului primit din Chat
public record OrderPayload(string ProductName, int Quantity);