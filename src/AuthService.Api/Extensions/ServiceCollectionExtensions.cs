using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Constants;
using AuthService.Persistence.Data;
using AuthService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
    IConfiguration configuration)
    {
        //Inicializacion con la base de datos
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());

        // Configure application services <------ ACTUALIZACIÓN
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAuthService, Application.Services.AuthService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
    
        //Inicializando el servicio Email
        services.AddScoped<IEmailService, EmailService>();

        services.AddHealthChecks();

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}

