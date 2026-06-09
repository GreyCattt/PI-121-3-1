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
        Pending,
        Active,
        Cancelled,
        Sold,
        NotSold
    }
}