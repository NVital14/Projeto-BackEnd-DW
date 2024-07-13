using Microsoft.AspNetCore.Identity;

using Projeto.Models;


namespace Projeto.Data
{
    internal class DbInitializer
    {

        internal static async Task Initialize(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
            ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));
            ArgumentNullException.ThrowIfNull(roleManager, nameof(roleManager));
            dbContext.Database.EnsureCreated();

            bool haAdicao = false;

            try
            {
                // verifica se há roles, senão cria
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                //a hasher to hash the password before seeding the user to the db
                var hasher = new PasswordHasher<IdentityUser>();
                // verifica se há utilizadores, senão cria
                if (!dbContext.Users.Any())
                {
                    var user = new IdentityUser
                    {
                        UserName = "admin@gmail.com",
                        NormalizedUserName = "ADMIN@GMAIL.COM   ",
                        Email = "admin@gmail.com",
                        NormalizedEmail = "admin@gmail.com",
                        EmailConfirmed = true,
                        SecurityStamp = "5ZPZEF6SBW7IU4M344XNLT4NN5RO4GRU",
                        ConcurrencyStamp = "c86d8254-dd50-44be-8561-d2d44d4bbb2f",
                        PasswordHash = hasher.HashPassword(null, "aAo1234."),
                    };

                    var result = await userManager.CreateAsync(user, "Admin@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                        haAdicao = true;
                    }
                }

                // verifica se há categorias na BD, senão cria
                if (!dbContext.Categories.Any())
                {
                    var category1 = new Categories
                    {
                        Name = "Filmes"
                    };

                    var category2 = new Categories
                    {
                        Name = "Livros"
                    };

                    dbContext.Categories.AddRange(category1, category2);
                    haAdicao = true;

                }

                if (haAdicao)
                {
                    // tornar persistentes os dados
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }
    }
}
