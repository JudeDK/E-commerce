using System.ComponentModel.DataAnnotations;

namespace ProiectWeb.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}