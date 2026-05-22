classDiagram
    direction TB

    %% Контекст БД та патерни доступу
    class AuctionDbContext {
        +DbSet~User~ Users
        +DbSet~Lot~ Lots
        +DbSet~Bid~ Bids
        +DbSet~Category~ Categories
        #OnModelCreating(ModelBuilder modelBuilder)
    }

    class IRepository~T~ {
        <<interface>>
        +GetAllAsync() Task~IEnumerable~T~~
        +GetByIdAsync(int id) Task~T?~
        +AddAsync(T entity) Task
        +Update(T entity) void
        +Delete(T entity) void
        +GetAsQueryable() IQueryable~T~
    }

    class Repository~T~ {
        -AuctionDbContext _context
        -DbSet~T~ _dbSet
        +Repository(AuctionDbContext context)
    }

    class IUnitOfWork {
        <<interface>>
        +UserRepository IRepository~User~
        +CategoryRepository IRepository~Category~
        +LotRepository IRepository~Lot~
        +BidRepository IRepository~Bid~
        +SaveChangesAsync() Task~int~
    }

    class UnitOfWork {
        -AuctionDbContext _context
        -IRepository~User~ _userRepository
        -IRepository~Category~ _categoryRepository
        -IRepository~Lot~ _lotRepository
        -IRepository~Bid~ _bidRepository
        +UnitOfWork(AuctionDbContext context)
    }

    %% Сутності (Entities)
    class User {
        +int Id
        +string Username
        +string Email
        +string PasswordHash
        +UserRole Role
        +ICollection~Lot~ CreatedLots
        +ICollection~Bid~ Bids
    }

    class Lot {
        +int Id
        +string Title
        +string Description
        +decimal StartingPrice
        +DateTime StartTime
        +DateTime EndTime
        +LotStatus Status
        +int CategoryId
        +Category Category
        +int SellerId
        +User Seller
        +int? ApprovedByManagerId
        +User ApprovedByManager
        +ICollection~Bid~ Bids
    }

    class Category {
        +int Id
        +string Name
        +int? ParentCategoryId
        +Category ParentCategory
        +ICollection~Category~ SubCategories
        +ICollection~Lot~ Lots
    }

    class Bid {
        +int Id
        +decimal Amount
        +DateTime Timestamp
        +int LotId
        +Lot Lot
        +int UserId
        +User User
    }

    %% Переліки (Enums)
    class UserRole {
        <<enum>>
        Admin
        Manager
        Registered
        Unregistered
    }

    class LotStatus {
        <<enum>>
        Pending
        Active
        Cancelled
        Sold
        NotSold
    }

    %% Допоміжні сервіси
    class PasswordHasher {
        <<static>>
        +Hash(string password)$ string
        +Verify(string password, string storedHash)$ bool
    }

    class SeedService {
        -AuctionDbContext _context
        +SeedAsync() Task
    }

    %% Зв'язки реалізації та залежностей
    IRepository~T~ <|.. Repository~T~
    IUnitOfWork <|.. UnitOfWork
    
    Repository~T~ --> AuctionDbContext : використовує
    UnitOfWork --> AuctionDbContext : керує життєвим циклом
    UnitOfWork "1" *-- "many" IRepository~T~ : містить репозиторії
    SeedService --> AuctionDbContext : заповнює даними

    %% Зв'язки між сутностями
    User --> UserRole
    Lot --> LotStatus
    Lot "many" --> "1" Category : входить до
    Lot "many" --> "1" User : належить Seller
    Lot "many" --> "0..1" User : підтверджується ApprovedByManager
    Category "many" --> "0..1" Category : ParentCategory (Self-reference)
    Bid "many" --> "1" Lot : зроблена на
    Bid "many" --> "1" User : належить покупцю