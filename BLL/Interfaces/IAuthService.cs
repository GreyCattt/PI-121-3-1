using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);

        Task<string> RegisterAsync(string username, string email, string password);

        Task<AuthenticatedUserDto> GetCurrentUserAsync(int userId);
    }
}