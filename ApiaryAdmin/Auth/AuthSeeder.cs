using ApiaryAdmin.Auth.Model;
using Microsoft.AspNetCore.Identity;

namespace ApiaryAdmin.Auth;

public class AuthSeeder
{
    private readonly UserManager<ApiaryUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthSeeder(UserManager<ApiaryUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    
    public async Task SeedAsync()
    {
        await AddDefaultRolesAsync();
        await AddAdminUserAsync();
    }

    private async Task AddAdminUserAsync()
    {
        var newAdminUser = new ApiaryUser
        {
            UserName = "adminas",
            Email = "admin@admin.com",
        };
        
        var existAdminUser = await _userManager.FindByNameAsync(newAdminUser.UserName);
        if (existAdminUser == null)
        {
            var createAdminUserResult = await _userManager.CreateAsync(newAdminUser, "Admin@123Admin");
            if (createAdminUserResult.Succeeded)
            {
                await _userManager.AddToRolesAsync(newAdminUser, ApiaryRoles.All);
            }
        }
    }

    private async Task AddDefaultRolesAsync()
    {
        foreach (var role in ApiaryRoles.All)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}