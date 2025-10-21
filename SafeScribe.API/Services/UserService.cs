using SafeScribe.API.Interfaces;
using SafeScribe.API.Models;
using SafeScribe.API.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Threading.Tasks;

namespace SafeScribe.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
            SeedData();
        }
        private void SeedData()
        {
            if (!_context.Users.Any())
            {
                _context.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin"
                });

                _context.Users.Add(new User
                {
                    Username = "editor",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Editor123!"),
                    Role = "Editor"
                });

                _context.SaveChanges();
            }
        }

        public async Task<User?> RegisterAsync(string username, string password, string role)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (existingUser != null)
            {
                return null;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Role = role
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return null;
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}