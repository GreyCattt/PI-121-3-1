using System;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> UserRepository { get; }
        IRepository<Category> CategoryRepository { get; }
        IRepository<Lot> LotRepository { get; }
        IRepository<Bid> BidRepository { get; }
        
        Task<int> SaveChangesAsync();
    }
}