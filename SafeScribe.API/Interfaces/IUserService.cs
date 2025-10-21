using SafeScribe.API.Models;

namespace SafeScribe.API.Interfaces
{
    public interface IUserService
    {
        Task<User?> RegisterAsync(string username, string password, string role);

        Task<User?> AuthenticateAsync(string username, string password);

        Task<User?> GetUserByIdAsync(int id);
    }
}