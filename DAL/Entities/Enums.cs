namespace DAL.Entities
{
    public enum UserRole
    {
        Admin,
        Manager,
        Registered,
        Unregistered
    }

    public enum LotStatus
    {
        Pending,   // Очікує підтвердження менеджером
        Active,    // Торги тривають
        Cancelled, // Скасовано
        Sold,      // Продано
        NotSold    // Торги завершені без покупця
    }
}