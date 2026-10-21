// ============================================================
// Program.cs - ASP.NET Core MVC on .NET 10
// Clean Architecture entry point and DI composition root.
// Replaces: Global.asax, App_Start/RouteConfig.cs,
//           App_Start/FilterConfig.cs, App_Start/BundleConfig.cs
// ============================================================
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Application.Common.Interfaces;
using OrbitAOS.V6.Data;
using OrbitAOS.V6.Infrastructure.Services;
using Serilog;

// -------------------------------------------------------
// 1. Bootstrap Serilog early so startup errors are logged
// -------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting OrbitAOS.V6 web host on .NET 10");

    var builder = WebApplication.CreateBuilder(args);

    // -------------------------------------------------------
    // 2. Serilog as the logging provider (replaces default)
    // -------------------------------------------------------
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

    // -------------------------------------------------------
    // 3. Data layer – EF Core 10 with SQL Server
    //    Connection string migrated from web.config to
    //    appsettings.json (ConnectionStrings:DefaultConnection)
    // -------------------------------------------------------
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' not found in appsettings.json.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            // Enable retry on transient failures (production resilience)
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
        // Enable sensitive data logging only in Development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // Developer exception page for EF Core migrations
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // -------------------------------------------------------
    // 4. ASP.NET Core Identity (migrated from ASP.NET Identity 2.x)
    //    RequireConfirmedAccount kept from original net6 baseline
    // -------------------------------------------------------
    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        // .NET 10 recommended password policy
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

    // -------------------------------------------------------
    // 5. Clean Architecture – Application layer registrations
    //    MediatR for CQRS (Commands / Queries)
    //    FluentValidation for request validation
    // -------------------------------------------------------
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    // -------------------------------------------------------
    // 6. Clean Architecture – Infrastructure layer registrations
    //    Register application service interfaces → implementations
    // -------------------------------------------------------
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IDateTimeService, DateTimeService>();
    builder.Services.AddHttpContextAccessor(); // Required by CurrentUserService

    // -------------------------------------------------------
    // 7. MVC with Views (replaces AddControllersWithViews from net6)
    //    .NET 10: AddControllersWithViews is still the correct call
    // -------------------------------------------------------
    builder.Services.AddControllersWithViews(options =>
    {
        // Global exception filter registered here (replaces FilterConfig.cs)
        // options.Filters.Add<GlobalExceptionFilter>();
    });

    // -------------------------------------------------------
    // 8. Response Caching middleware
    //    Replaces [OutputCache] attribute from legacy System.Web
    // -------------------------------------------------------
    builder.Services.AddResponseCaching();

    // -------------------------------------------------------
    // 9. Session state (ASP.NET Core distributed session)
    //    Replaces System.Web.SessionState
    // -------------------------------------------------------
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

    // -------------------------------------------------------
    // 10. Health checks
    // -------------------------------------------------------
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>();

    // -------------------------------------------------------
    // 11. Build the application
    // -------------------------------------------------------
    var app = builder.Build();

    // -------------------------------------------------------
    // 12. HTTP request pipeline
    //     Order matters: exception handling → HSTS → HTTPS →
    //     static files → routing → auth → endpoints
    // -------------------------------------------------------
    if (app.Environment.IsDevelopment())
    {
        // Shows EF Core migration errors in browser during development
        app.UseMigrationsEndPoint();
    }
    else
    {
        // Production error handler (replaces Application_Error in Global.asax)
        app.UseExceptionHandler("/Home/Error");
        // HSTS: 30-day default; adjust for production
        // See https://aka.ms/aspnetcore-hsts
        app.UseHsts();
    }

    // Redirect HTTP → HTTPS (replaces web.config requireSSL)
    app.UseHttpsRedirection();

    // Serve static files from wwwroot (replaces /Content and /Scripts)
    app.UseStaticFiles();

    // Serilog request logging
    app.UseSerilogRequestLogging();

    // Response caching middleware
    app.UseResponseCaching();

    // Session middleware (must be before routing for session access in controllers)
    app.UseSession();

    // Routing middleware (replaces RouteConfig.cs RegisterRoutes)
    app.UseRouting();

    // Authentication & Authorization middleware
    // (replaces Forms Authentication in web.config + FilterConfig.cs)
    app.UseAuthentication();
    app.UseAuthorization();

    // Health check endpoint
    app.MapHealthChecks("/health");

    // -------------------------------------------------------
    // 13. Route configuration
    //     Replaces App_Start/RouteConfig.cs MapRoute calls
    //     .NET 10 uses MapControllerRoute (attribute routing
    //     is also fully supported via [Route] attributes)
    // -------------------------------------------------------
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Razor Pages for Identity UI scaffolding
    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrbitAOS.V6 host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
