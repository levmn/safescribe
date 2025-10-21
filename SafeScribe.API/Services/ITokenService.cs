using SafeScribe.API.Models;

namespace SafeScribe.API.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
