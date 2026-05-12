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

            // Налаштування User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Налаштування Category (Деревоподібна структура)
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);

                // Зв'язок Parent-Child (самопосилання)
                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Налаштування Lot
            modelBuilder.Entity<Lot>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Title).IsRequired().HasMaxLength(200);
                entity.Property(l => l.StartingPrice).HasPrecision(18, 2);

                // Зв'язок з категорією
                entity.HasOne(l => l.Category)
                    .WithMany(c => c.Lots)
                    .HasForeignKey(l => l.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Зв'язок з продавцем
                entity.HasOne(l => l.Seller)
                    .WithMany(u => u.CreatedLots)
                    .HasForeignKey(l => l.SellerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Зв'язок з менеджером (опціонально)
                entity.HasOne(l => l.ApprovedByManager)
                    .WithMany()
                    .HasForeignKey(l => l.ApprovedByManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Налаштування Bid (Ставки)
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Amount).HasPrecision(18, 2);

                entity.HasOne(b => b.Lot)
                    .WithMany(l => l.Bids)
                    .HasForeignKey(b => b.LotId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bids)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Примітка: початкові дані (seed) тепер додаються програмно в SeedService,
            // щоб уникнути конфліктів при міграціях і дублюванні PK.
        }
    }
}