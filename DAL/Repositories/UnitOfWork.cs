using System;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;

namespace DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AuctionDbContext _context;
        private IRepository<User>? _userRepository;
        private IRepository<Category>? _categoryRepository;
        private IRepository<Lot>? _lotRepository;
        private IRepository<Bid>? _bidRepository;

        public UnitOfWork(AuctionDbContext context)
        {
            _context = context;
        }

        public IRepository<User> UserRepository => _userRepository ??= new Repository<User>(_context);
        public IRepository<Category> CategoryRepository => _categoryRepository ??= new Repository<Category>(_context);
        public IRepository<Lot> LotRepository => _lotRepository ??= new Repository<Lot>(_context);
        public IRepository<Bid> BidRepository => _bidRepository ??= new Repository<Bid>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}