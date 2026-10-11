using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.App;
using TaskManager.App.Data;
using TaskManager.App.Features.Identity;
using TaskManager.App.Features.Tasks;
using TaskManager.Web.Components;
using TaskManager.Web.Components.Account;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddTaskManagerApp(connectionString);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 6;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

await DatabaseSeeder.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Bypass precompressed static-web-asset pipeline (can serve empty app.css under gzip).
app.MapGet("/theme.css", (IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.WebRootPath, "app.css");
    return Results.File(path, "text/css");
});

app.MapAdditionalIdentityEndpoints();

app.MapGet("/api/tasks/export", async (
    [FromServices] TaskService taskService,
    [FromServices] TaskExcelExportService excelExport,
    [FromServices] UserDirectoryService users,
    string? search,
    int? companyId,
    string? assignedUserId,
    bool onlyOverdue,
    bool openOnly,
    bool highPriorityOnly,
    CancellationToken ct) =>
{
    var result = await taskService.SearchAsync(new TaskQuery
    {
        Search = search,
        CompanyId = companyId,
        AssignedUserId = assignedUserId,
        OnlyOverdue = onlyOverdue,
        OpenOnly = openOnly,
        HighPriorityOnly = highPriorityOnly,
        Page = 1,
        PageSize = 1000
    }, ct);

    var names = await users.GetDisplayMapAsync(result.Items.Select(t => t.AssignedUserId), ct);
    var bytes = excelExport.Export(result.Items, names);
    return Results.File(
        bytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"tasks-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx");
}).RequireAuthorization();

app.Run();

public partial class Program;
