using System;

namespace BLL.Exceptions
{
    public class AuctionValidationException : Exception
    {
        public AuctionValidationException(string message) : base(message)
        {
        }
    }
}