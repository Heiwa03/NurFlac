using Microsoft.Extensions.DependencyInjection;
using NurFlac.UserManagement;

namespace NurFlac.Extensions;

public static class UserManagementServiceExtensions
{
    public static IServiceCollection AddUserManagement(this IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, SqliteUserRepository>();
        services.AddSingleton<IUserService, UserService>();
        return services;
    }
}
