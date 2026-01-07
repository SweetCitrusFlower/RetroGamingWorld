using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RetroGamingWorld.Data;
using System.Collections.Generic;
using System.Globalization;

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

                context.Categories.AddRange(
                    new Category
                    {
                        CategoryName = "PS4"
                    },
                    new Category
                    {
                        CategoryName = "PS5",
                    },
                    new Category
                    {
                        CategoryName = "SNES"
                    },
                    new Category
                    {
                        CategoryName = "XBOX"
                    },
                    new Category
                    {
                        CategoryName = "Gameboy Advanced"
                    },
                    new Category
                    {
                        CategoryName = "Sega Dreamcast"
                    },
                    new Category
                    {
                        CategoryName = "Sega Genesis"
                    }
                );

                context.Articles.AddRange(
                    new Article
                    {
                        Title = "Sonic the Hedgehog 2",
                        Content = "Sonic the Hedgehog 2 este un joc platformă din 1992 dezvoltat de Institutul Tehnic Sega (STI) pentru Sega Genesis. Jucătorii îl controlează pe Sonic în timp ce încearcă să-l împiedice pe Doctor Robotnik să fure Chaos Emeralds pentru a-și alimenta stația spațială, Death Egg. La fel ca primul Sonic the Hedgehog (1991), jucătorii parcurg niveluri de defilare laterală cu viteză mare în timp ce colectează inele, înving inamicii și luptă cu șefii. Sonic 2 îl prezintă pe prietenul lui Sonic, Miles „Tails” Prower și oferă un joc mai rapid, niveluri mai mari, un mod multiplayer și etape speciale cu grafică 3D pre-redată.",
                        Date = DateTime.ParseExact("21-11-1992", "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        Image = "https://m.media-amazon.com/images/M/MV5BMzg3OGY0MzMtMWM1My00YzBmLTg4ZTktZTk0ZWU1ODY3YmU2XkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg",
                        Price = (decimal)109.00,
                        Stock = 50,
                        Rating = 0,
                        IsApproved = true,
                        AdminFeedback = null,
                        CategoryId = 7,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new Article
                    {
                        Title = "Sonic the Hedgehog 1",
                        Content = "Sonic the Hedgehog este un joc platformă din 1991 dezvoltat și publicat de Sega pentru Sega Genesis. Jucătorul îl controlează pe Sonic, un arici care poate alerga la viteze supersonice. Povestea îl urmărește pe Sonic în timp ce acesta își propune să dejuteze planurile omului de știință nebun Doctor Eggman de a căuta puternicele Smaralde Chaos. Jocul implică colectarea inelelor ca formă de sănătate și o schemă simplă de control, cu sărituri și atacuri controlate de un singur buton.",
                        Date = DateTime.ParseExact("21-06-1991", "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        Image = "https://i1.sndcdn.com/artworks-000256259864-uuqj58-t500x500.jpg",
                        Price = (decimal)89.00,
                        Stock = 50,
                        Rating = 0,
                        IsApproved = true,
                        AdminFeedback = null,
                        CategoryId = 7,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new Article
                    {
                        Title = "Pokémon Emerald",
                        Content = "Pokémon Emerald este un joc video de rol din 2004 dezvoltat de Game Freak și publicat de The Pokémon Company și Nintendo pentru Game Boy Advance. Jucătorii controlează un antrenor Pokémon dintr-o perspectivă de deasupra capului. Obiectivul general al jucătorului este să exploreze regiunea Hoenn și să cucerească o serie de opt săli de sport Pokémon pentru a-i provoca pe Elite Four și pe Campionul Ligii Pokémon din Hoenn, în timp ce subplotul principal este să învingă două organizații criminale care încearcă să valorifice puterea unui Pokémon legendar pentru propriile lor obiective.",
                        Date = DateTime.ParseExact("16-09-2004", "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        Image = "https://upload.wikimedia.org/wikipedia/en/f/f7/PokemonEmeraldBox.jpg",
                        Price = (decimal)129.99,
                        Stock = 5,
                        Rating = 0,
                        IsApproved = true,
                        AdminFeedback = null,
                        CategoryId = 5,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                    },
                    new Article
                    {
                        Title = "Super Mario World",
                        Content = "Super Mario World este un joc platformă dezvoltat și publicat de Nintendo pentru Super Nintendo Entertainment System (SNES). Jucătorul îl controlează pe Mario în încercarea sa de a salva Prințesa Peach și Dinosaur Land de antagonistul seriei Bowser și Koopalings. Jucătorii îl controlează pe Mario printr-o serie de niveluri în care scopul este să ajungă la stâlpul porții la final. Super Mario World îl prezintă pe Yoshi, un dinozaur care poate fi călărit, care poate mânca dușmani și îi poate scuipa pe unii ca proiectile.",
                        Date = DateTime.ParseExact("21-11-1990", "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        Image = "https://i.ebayimg.com/images/g/-P0AAOSw0theMQfB/s-l1200.jpg",
                        Price = (decimal)59.00,
                        Stock = 80,
                        Rating = 0,
                        IsApproved = true,
                        AdminFeedback = null,
                        CategoryId = 3,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3"
                    },
                    new Article
                    {
                        Title = "Super Metroid",
                        Content = "Super Metroid este un joc de acțiune și aventură din 1994, dezvoltat de Nintendo și Intelligent Systems și publicat de Nintendo pentru Super Nintendo Entertainment System (SNES). Este al treilea joc Metroid, după jocul Game Boy Metroid II: Return of Samus (1991). Jucătorul îl controlează pe vânătorul de recompense Samus Aran, care călătorește pe planeta Zebes pentru a recupera o creatură Metroid furată de liderul piraților spațiali Ridley.",
                        Date = DateTime.ParseExact("19-03-1994", "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        Image = "https://assets-prd.ignimgs.com/2021/12/07/supermetroid-1638920229201.jpg",
                        Price = (decimal)89.00,
                        Stock = 50,
                        Rating = 0,
                        IsApproved = true,
                        AdminFeedback = null,
                        CategoryId = 3,
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3"
                    }
                    //,
                    //new Article
                    //{
                    //    Title = "",
                    //    Content = "",
                    //    Date = DateTime.ParseExact("", "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    //    Image = "",
                    //    Price = (decimal)89.00,
                    //    Stock = 50,
                    //    Rating = 0,
                    //    IsApproved = true,
                    //    AdminFeedback = null,
                    //    CategoryId = ,
                    //    UserId = ""
                    //}

                );
                context.SaveChanges();
            }
        }
    }
}
