using System;

namespace BLL.DTOs
{
    public class BidDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}