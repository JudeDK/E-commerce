using ProiectWeb.Data;
using ProiectWeb.Models;

namespace ProiectWeb.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(string message)
    {
        _db.Notifications.Add(new Notification
        {
            Message = message,
            DateCreated = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }
}
