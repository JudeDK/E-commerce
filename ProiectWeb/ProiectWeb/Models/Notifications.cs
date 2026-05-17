using System;

namespace ProiectWeb.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = null!;
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
