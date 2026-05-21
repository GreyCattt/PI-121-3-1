using System;
using DAL.Entities;

namespace PL.Models
{
    public class UpdateLotRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int CategoryId { get; set; }
        public LotStatus Status { get; set; }
    }
}