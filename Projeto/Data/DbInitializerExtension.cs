using Microsoft.AspNetCore.Identity;

namespace Projeto.Data
{
    internal static class DbInitializerExtension
    {
        public static async Task<IApplicationBuilder> UseItToSeedSqlServer(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
               await DbInitializer.Initialize(context, userManager, roleManager);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao fazer seed.", ex);
            }

            return app;
        }
    
}
}
