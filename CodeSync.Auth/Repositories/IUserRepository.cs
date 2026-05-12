using CodeSync.Auth.Models;

namespace CodeSync.Auth.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByUsernameAsync(string username);
        Task<User?> FindByUserIdAsync(int userId);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<IEnumerable<User>> FindAllByRoleAsync(string role);
        Task<IEnumerable<User>> SearchByUsernameAsync(string query);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteByUserIdAsync(int userId);
    }
}