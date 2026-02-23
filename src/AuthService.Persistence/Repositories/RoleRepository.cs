using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    // Inyectamos el contexto de la base de datos
    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    // Corregido el nombre: CountUsersInRoleAsync (con 's') 
    // y usando roleId como pide la interfaz
    public async Task<int> CountUsersInRoleAsync(string roleId)
    {
        return await _context.UserRoles
            .Where(ur => ur.RoleId == roleId) 
            .CountAsync();
    }

    // Ajustado para que reciba roleId y devuelva la lista correctamente
    public async Task<IReadOnlyList<User>> GetUserByRoleAsync(string roleId)
    {
        var users = await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.User)
            .Include(u => u.UserProfile)
            .Include(u => u.UserEmail)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();

        return users.AsReadOnly();
    }

    // Corregido el nombre: GetUserRoleNameAsync (sin la 's' al final de Name)
    public async Task<IReadOnlyList<string>> GetUserRoleNameAsync(string userId)
    {
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        return roles.AsReadOnly();
    }
}