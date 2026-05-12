using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        // Метод приймає email (або username) та пароль, а повертає рядок (JWT токен)
        Task<string> LoginAsync(string email, string password);

        // Реєстрація нового користувача з роллю Registered
        Task<string> RegisterAsync(string username, string email, string password);

        // Повертає профіль поточного користувача за його Id із JWT
        Task<AuthenticatedUserDto> GetCurrentUserAsync(int userId);
    }
}