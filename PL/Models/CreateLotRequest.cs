using System;

namespace PL.Models
{
    public class CreateLotRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int CategoryId { get; set; }

        // В реальному проєкті ID продавця зазвичай береться з токена авторизації (JWT), 
        // але для спрощення на цьому етапі будемо передавати його в тілі запиту.
        public int SellerId { get; set; }
    }
}