using System.Text.Json.Serialization;
using Blackhood.Api.Characters;
using Blackhood.Api.Endpoints;
using Blackhood.Data;
using Blackhood.Data.Characters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<BlackhoodDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Blackhood")));

builder.Services.Configure<ContentOptions>(builder.Configuration.GetSection("Content"));
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection("Admin"));
builder.Services.AddSingleton<ContentPaths>();
builder.Services.AddScoped<TableCatalog>();

// Registration / login: ASP.NET Core Identity with a cookie (the React app is served from the same origin via the Vite proxy).
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityCore<AppUser>(o =>
    {
        o.User.RequireUniqueEmail = true;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequireUppercase = false;
        if (builder.Environment.IsDevelopment())
        {
            // While testing: any password is fine (even "1").
            o.Password.RequiredLength = 1;
            o.Password.RequiredUniqueChars = 1;
            o.Password.RequireDigit = false;
            o.Password.RequireLowercase = false;
        }
        else
        {
            o.Password.RequiredLength = 8;
        }
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<BlackhoodDbContext>()
    .AddSignInManager();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.Name = "blackhood.auth";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.ExpireTimeSpan = TimeSpan.FromDays(14);
    o.SlidingExpiration = true;
    // An API answers 401/403 instead of redirecting to a login page.
    o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<BlackhoodDbContext>();

    // Code-first: apply pending migrations on start in development.
    await db.Database.MigrateAsync();

    // Seed characters from content/characters/*.json that are not in the database yet.
    var content = app.Configuration.GetSection("Content").Get<ContentOptions>() ?? new ContentOptions();
    if (content.ImportOnStartup)
    {
        var imported = await CharacterContent.ImportMissingAsync(db, services.GetRequiredService<ContentPaths>().Characters);
        if (imported > 0) app.Logger.LogInformation("Imported {Count} characters from content files.", imported);
    }

    await AdminAccess.SeedAsync(services, app.Configuration.GetSection("Admin").Get<AdminOptions>() ?? new AdminOptions());
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCharacterEndpoints();
app.MapTavernEndpoints();
app.MapGiftsEndpoints();
app.MapBonesEndpoints();
app.MapAdminEndpoints();
app.MapLogEndpoints();

app.Run();
