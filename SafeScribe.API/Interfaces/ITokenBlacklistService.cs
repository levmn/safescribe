namespace SafeScribe.API.Interfaces
{
    public interface ITokenBlacklistService
    {
        void AddToBlacklist(string jti);

        bool IsBlacklisted(string jti);
    }
}