using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        // Метод приймає email (або username) та пароль, а повертає рядок (JWT токен)
        Task<string> LoginAsync(string email, string password);
    }
}