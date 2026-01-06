using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetroGamingWorld.Data;
using System.Collections.Generic;

namespace RetroGamingWorld.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AppDbContext(
                serviceProvider.GetRequiredService 
                <DbContextOptions<AppDbContext>>()))
            {
                if (context.Roles.Any())
                {
                    return;
                }

                context.Roles.AddRange(
                    new IdentityRole
                    { 
                        Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Administrator", NormalizedName = "Administrator".ToUpper() 
                    },
                    new IdentityRole
                    {
                        Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "Colaborator", NormalizedName = "Colaborator".ToUpper() 
                    },
                    new IdentityRole
                    {
                        Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "User", NormalizedName = "User".ToUpper() 
                    }
                );

                var hasher = new PasswordHasher<ApplicationUser>();

                context.Users.AddRange(
                    new ApplicationUser
                    {

                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                        // primary key
                        UserName = "admin@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        PasswordHash = hasher.HashPassword(null,"Admin1!")
                    },

                    new ApplicationUser
                    {

                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                        // primary key
                        UserName = "colab@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "COLABO@TEST.COM",
                        Email = "colab@test.com",
                        NormalizedUserName = "COLAB@TEST.COM",
                        PasswordHash = hasher.HashPassword(null,"Colab1!")
                    },

                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb3",
                        UserName = "colab2@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "COLAB2@TEST.COM",
                        Email = "colab2@test.com",
                        NormalizedUserName = "COLAB2@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Colab2!")
                    },

                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                        // primary key
                        UserName = "user@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER@TEST.COM",
                        Email = "user@test.com",
                        NormalizedUserName = "USER@TEST.COM",
                        PasswordHash = hasher.HashPassword(null,"User1!")
                    },

                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                        // primary key
                        UserName = "user2@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER2@TEST.COM",
                        Email = "user2@test.com",
                        NormalizedUserName = "USER2@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "User2!")
                    }
                );

                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                    },

                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },

                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3"
                    },

                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                    },

                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
