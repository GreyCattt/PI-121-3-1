namespace PL.Models
{
    public class PlaceBidRequest
    {
        public int LotId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}