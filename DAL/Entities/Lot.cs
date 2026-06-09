using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class Lot
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public LotStatus Status { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public int SellerId { get; set; }
        public virtual User Seller { get; set; } = null!;

        public int? ApprovedByManagerId { get; set; }
        public virtual User? ApprovedByManager { get; set; }

        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}