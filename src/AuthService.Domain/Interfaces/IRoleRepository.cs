using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
    Task<int> CountUsersInRoleAsync(string roleId);
    //Los usuarios que tienen ese rol
    Task<IReadOnlyList<User>> GetUserByRoleAsync(string roleId); 
    //Roles disponibles
    Task<IReadOnlyList<string>> GetUserRoleNameAsync(string userId); 
}