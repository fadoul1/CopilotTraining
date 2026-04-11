using LeaveManagement.Domain.Contracts.Repositories;
using LeaveManagement.Domain.Contracts.Services;
using LeaveManagement.Infrastructure.Services;
using LeaveManagement.Infrastructure.Data;
using LeaveManagement.Infrastructure.Repositories;
using LeaveManagement.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveManagement.Infrastructure;

public static class InfrastructureServicesRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<ApplicationContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("LeaveManagementConnectionString"))
        );

        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<ILeaveRepository, LeaveRepository>();
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();

        return services;
    }
}
