using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.App.Data;
using TaskManager.App.Features.Catalogs;
using TaskManager.App.Features.Identity;
using TaskManager.App.Features.Improvements;
using TaskManager.App.Features.Tasks;
using TaskManager.App.Features.TimeTracking;

namespace TaskManager.App;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskManagerApp(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<CompanyService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<SubProjectService>();
        services.AddScoped<UserDirectoryService>();
        services.AddScoped<TaskService>();
        services.AddScoped<TimeEntryService>();
        services.AddScoped<ImprovementService>();
        services.AddSingleton<TaskExcelExportService>();

        return services;
    }
}
