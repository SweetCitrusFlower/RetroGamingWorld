using Microsoft.AspNetCore.Identity;
using RetroGamingWorld;
using System.Threading.Tasks;

public static class DataSeeder
{
    private const string AdminRole = "Administrator";
    private const string UserRole = "User";

    public static async Task InitializeRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (await roleManager.FindByNameAsync(AdminRole) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        if (await roleManager.FindByNameAsync(UserRole) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(UserRole));
        }
    }
}