using Microsoft.Extensions.DependencyInjection;
using USUN2.Business.Abstractions;
using USUN2.Business.Services;
using USUN2.Data.Abstractions;
using USUN2.Data.Infrastructure;
using USUN2.Data.Repositories;

namespace USUN2.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancePreferenceStack(this IServiceCollection services)
    {
        services.AddSingleton<SqlConnectionFactory>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILikeListRepository, LikeListRepository>();
        services.AddScoped<ILikeListService, LikeListService>();
        services.AddScoped<IAppUserManagementService, AppUserManagementService>();
        return services;
    }
}
