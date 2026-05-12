using CodeSync.Auth.Data;
using CodeSync.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Auth.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;

        public UserRepository(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<User?> FindByEmailAsync(string email) =>
            await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> FindByUsernameAsync(string username) =>
            await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

        public async Task<User?> FindByUserIdAsync(int userId) =>
            await _db.Users.FindAsync(userId);

        public async Task<bool> ExistsByEmailAsync(string email) =>
            await _db.Users.AnyAsync(u => u.Email == email);

        public async Task<bool> ExistsByUsernameAsync(string username) =>
            await _db.Users.AnyAsync(u => u.Username == username);

        public async Task<IEnumerable<User>> FindAllByRoleAsync(string role) =>
            await _db.Users
                .Where(u => u.Role == role)
                .ToListAsync();

        public async Task<IEnumerable<User>> SearchByUsernameAsync(string query) =>
            await _db.Users
                .Where(u => u.Username.Contains(query) && u.IsActive)
                .Take(20)
                .ToListAsync();

        public async Task<User> CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task DeleteByUserIdAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is not null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
        }
    }
}