using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IUserRepository
{
    //Metodos de Consulta
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<User> DeleteAsync(User user);

    Task<bool> ExistByEmailAsync(string email);
    Task<bool> ExistByUsernameAsync(string username);
    Task UpdateUserRoleAsync(string userId, string roleId);
    
}