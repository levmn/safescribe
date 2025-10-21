using SafeScribe.API.Models;

namespace SafeScribe.API.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
