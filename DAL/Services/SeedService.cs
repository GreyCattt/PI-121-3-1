using System;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using DAL.Services;

namespace DAL.Services
{
    public class SeedService
    {
        private readonly AuctionDbContext _context;

        public SeedService(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();

                var adminExists = await _context.Users.AnyAsync(u => u.Email == "admin@auction.com");
                if (!adminExists)
                {
                    Console.WriteLine("🌱 Додавання дефолтного адміністратора...");

                    var admin = new User
                    {
                        Username = "SuperAdmin",
                        Email = "admin@auction.com",
                        PasswordHash = PasswordHasher.Hash("Admin123!"),
                        Role = UserRole.Admin
                    };

                    _context.Users.Add(admin);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("✅ Дефолтного адміністратора додано.");
                }

                if (await _context.Users.CountAsync() > 1)
                {
                    Console.WriteLine("✅ У БД вже є користувачі, seed демо-даних пропущено.");
                    return;
                }

                Console.WriteLine("🌱 Додавання тестових даних...");

                var seededAdmin = await _context.Users.FirstAsync(u => u.Email == "admin@auction.com");

                var seller = new User
                {
                    Username = "john_seller",
                    Email = "john@example.com",
                    PasswordHash = PasswordHasher.Hash("John123!"),
                    Role = UserRole.Registered
                };

                var manager = new User
                {
                    Username = "manager_admin",
                    Email = "manager@example.com",
                    PasswordHash = PasswordHasher.Hash("Manager123!"),
                    Role = UserRole.Manager
                };

                var buyer = new User
                {
                    Username = "buyer_user",
                    Email = "buyer@example.com",
                    PasswordHash = PasswordHasher.Hash("Buyer123!"),
                    Role = UserRole.Registered
                };

                _context.Users.AddRange(seller, manager, buyer);
                await _context.SaveChangesAsync();

                var electronicsCategory = new Category
                {
                    Name = "Electronics",
                    ParentCategoryId = null
                };

                var phoneCategory = new Category
                {
                    Name = "Phones",
                    ParentCategory = electronicsCategory
                };

                var watchCategory = new Category
                {
                    Name = "Watches",
                    ParentCategory = electronicsCategory
                };

                _context.Categories.AddRange(electronicsCategory, phoneCategory, watchCategory);
                await _context.SaveChangesAsync();

                var now = DateTime.UtcNow;

                var lot1 = new Lot
                {
                    Title = "iPhone 15 Pro",
                    Description = "Смартфон Apple iPhone 15 Pro, 256GB, Silver - практично новий",
                    StartingPrice = 1200,
                    StartTime = now.AddDays(1),
                    EndTime = now.AddDays(7),
                    Status = LotStatus.Pending,
                    CategoryId = phoneCategory.Id,
                    SellerId = seller.Id
                };

                var lot2 = new Lot
                {
                    Title = "Gold Luxury Watch",
                    Description = "Елегантний золотий наручний годинник з швейцарським механізмом, оригінал",
                    StartingPrice = 500,
                    StartTime = now.AddHours(2),
                    EndTime = now.AddDays(14),
                    Status = LotStatus.Active,
                    CategoryId = watchCategory.Id,
                    SellerId = seller.Id,
                    ApprovedByManagerId = manager.Id
                };

                var lot3 = new Lot
                {
                    Title = "Dell XPS 15 Laptop",
                    Description = "Потужний ноутбук для роботи та ігор, Intel i7, 32GB RAM, 1TB SSD",
                    StartingPrice = 1500,
                    StartTime = now.AddDays(2),
                    EndTime = now.AddDays(10),
                    Status = LotStatus.Active,
                    CategoryId = electronicsCategory.Id,
                    SellerId = seller.Id,
                    ApprovedByManagerId = manager.Id
                };

                var lot4 = new Lot
                {
                    Title = "Samsung Galaxy S24",
                    Description = "Топовий смартфон Samsung Galaxy S24 Ultra, чорний колір",
                    StartingPrice = 900,
                    StartTime = now.AddDays(1),
                    EndTime = now.AddDays(5),
                    Status = LotStatus.Pending,
                    CategoryId = phoneCategory.Id,
                    SellerId = seller.Id
                };

                _context.Lots.AddRange(lot1, lot2, lot3, lot4);
                await _context.SaveChangesAsync();

                var bid1 = new Bid
                {
                    LotId = lot2.Id,
                    UserId = buyer.Id,
                    Amount = 550,
                    Timestamp = now.AddHours(1)
                };

                var bid2 = new Bid
                {
                    LotId = lot3.Id,
                    UserId = buyer.Id,
                    Amount = 1600,
                    Timestamp = now.AddHours(3)
                };

                _context.Bids.AddRange(bid1, bid2);
                await _context.SaveChangesAsync();

                Console.WriteLine("✅ Тестові дані успішно додані в БД!");
                Console.WriteLine($"   - Користувачів: 4 (admin, seller, manager, buyer)");
                Console.WriteLine($"   - Категорій: 3 (Electronics, Phones, Watches)");
                Console.WriteLine($"   - Лотів: 4");
                Console.WriteLine($"   - Ставок: 2");
            }
            catch (InvalidOperationException dbEx) when (dbEx.Message.Contains("Connection"))
            {
                Console.WriteLine($"⚠️  Помилка підключення до БД: {dbEx.Message}");
                Console.WriteLine("ℹ️  Переконайтеся, що SQL Server запущений.");
                Console.WriteLine("   Для запуску використайте: docker-compose up -d");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка при додаванні тестових даних: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
