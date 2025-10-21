using SafeScribe.API.Interfaces;
using System.Collections.Concurrent;

namespace SafeScribe.API.Services
{
    public class InMemoryTokenBlacklistService : ITokenBlacklistService
    {
        private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new ConcurrentDictionary<string, DateTime>();

        public void AddToBlacklist(string jti)
        {
            _blacklistedTokens.TryAdd(jti, DateTime.MaxValue);
        }

        public bool IsBlacklisted(string jti)
        {
            return _blacklistedTokens.ContainsKey(jti);
        }
    }
}