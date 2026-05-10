using System.Collections.Generic;

namespace DAL.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // Навігаційні властивості
        public virtual ICollection<Lot> CreatedLots { get; set; } = new List<Lot>();
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}