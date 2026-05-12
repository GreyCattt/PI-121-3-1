using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
        {
        }

        // Твої таблиці
        public DbSet<User> Users { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Category> Categories { get; set; }

        // ДОДАНО: Метод для початкового заповнення бази
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Створюємо готового Адміна
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1, // Обов'язково вказуємо Id = 1
                    Username = "SuperAdmin",
                    Email = "admin@auction.com", // Це логін адміна
                    PasswordHash = "Admin123!",  // Це пароль адміна
                    Role = UserRole.Admin        // Даємо йому найвищі права
                }
            );
        }
    }
}