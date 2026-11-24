using System.Security.Claims;
using Blackhood.Data;
using Microsoft.AspNetCore.Identity;

namespace Blackhood.Api.Characters;

public sealed class AdminOptions
{
    /// <summary>User names that get the Admin role automatically (startup and registration).</summary>
    public List<string> Users { get; set; } = [];
}

public static class AdminAccess
{
    public const string Role = "Admin";

    /// <summary>
    /// Checks the role in the database on every request (not the cookie claims), so a user
    /// promoted or demoted takes effect immediately without signing in again.
    /// </summary>
    public static RouteGroupBuilder RequireAdmin(this RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.AddEndpointFilter(async (ctx, next) =>
        {
            var users = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
            var user = await users.GetUserAsync(ctx.HttpContext.User);
            if (user is null || !await users.IsInRoleAsync(user, Role))
                return Results.Problem("Само за администратори.", statusCode: StatusCodes.Status403Forbidden);
            return await next(ctx);
        });
        return group;
    }

    public static async Task<bool> IsAdminAsync(this UserManager<AppUser> users, ClaimsPrincipal principal)
    {
        var user = await users.GetUserAsync(principal);
        return user is not null && await users.IsInRoleAsync(user, Role);
    }

    /// <summary>Ensures the role exists and the configured users have it.</summary>
    public static async Task SeedAsync(IServiceProvider services, AdminOptions options)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync(Role))
            await roles.CreateAsync(new IdentityRole(Role));

        var users = services.GetRequiredService<UserManager<AppUser>>();
        foreach (var name in options.Users)
            if (await users.FindByNameAsync(name) is { } user && !await users.IsInRoleAsync(user, Role))
                await users.AddToRoleAsync(user, Role);
    }
}
