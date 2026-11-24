using System.Security.Claims;
using Blackhood.Api.Characters;
using Blackhood.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Blackhood.Api.Endpoints;

public sealed record RegisterRequest(string UserName, string Email, string Password);
public sealed record LoginRequest(string UserName, string Password);
public sealed record UserResponse(string Id, string UserName, string Email, bool IsAdmin);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/register", async (RegisterRequest req, UserManager<AppUser> users, SignInManager<AppUser> signIn, IOptions<AdminOptions> admin) =>
        {
            var user = new AppUser { UserName = req.UserName.Trim(), Email = req.Email.Trim() };
            var result = await users.CreateAsync(user, req.Password);
            if (!result.Succeeded)
                return Results.ValidationProblem(result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));

            if (admin.Value.Users.Contains(user.UserName, StringComparer.OrdinalIgnoreCase))
                await users.AddToRoleAsync(user, AdminAccess.Role);

            await signIn.SignInAsync(user, isPersistent: true);
            return Results.Ok(await ToResponse(user, users));
        });

        auth.MapPost("/login", async (LoginRequest req, UserManager<AppUser> users, SignInManager<AppUser> signIn) =>
        {
            // Allow login by user name or e-mail.
            var user = await users.FindByNameAsync(req.UserName.Trim())
                       ?? await users.FindByEmailAsync(req.UserName.Trim());
            if (user is null)
                return Results.Problem("Грешно име или парола.", statusCode: StatusCodes.Status401Unauthorized);

            var result = await signIn.PasswordSignInAsync(user, req.Password, isPersistent: true, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Results.Problem(
                    result.IsLockedOut ? "Профилът е временно заключен." : "Грешно име или парола.",
                    statusCode: StatusCodes.Status401Unauthorized);

            return Results.Ok(await ToResponse(user, users));
        });

        auth.MapPost("/logout", async (SignInManager<AppUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.NoContent();
        });

        auth.MapGet("/me", async (ClaimsPrincipal principal, UserManager<AppUser> users) =>
        {
            var user = await users.GetUserAsync(principal);
            return user is null ? Results.Unauthorized() : Results.Ok(await ToResponse(user, users));
        }).RequireAuthorization();
    }

    private static async Task<UserResponse> ToResponse(AppUser u, UserManager<AppUser> users) =>
        new(u.Id, u.UserName ?? "", u.Email ?? "", await users.IsInRoleAsync(u, AdminAccess.Role));
}
