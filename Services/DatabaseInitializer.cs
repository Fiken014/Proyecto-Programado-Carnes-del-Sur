using CarnesDelSurMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace CarnesDelSurMVC.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDataAsync(UserManager<ApplicationUser>? userManager,
            RoleManager<IdentityRole>? roleManager)
        {
            if (userManager == null || roleManager == null)
            {
                Console.WriteLine("userManager o roleManager es nulo => salir");
                return;
            }

            // Comprobar si tenemos el rol de administrador o no
            var exists = await roleManager.RoleExistsAsync("admin");
            if (!exists)
            {
                Console.WriteLine("El rol de administrador no está definido y se creara");
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }

            // Comprobar si tenemos el rol de cliente o no
            exists = await roleManager.RoleExistsAsync("cliente");
            if (!exists)
            {
                Console.WriteLine("El rol de cliente no está definido y se creara");
                await roleManager.CreateAsync(new IdentityRole("cliente"));
            }


            // Comprobar si tenemos al menos un usuario administrador o no
            var adminUsers = await userManager.GetUsersInRoleAsync("admin");
            if (adminUsers.Any())
            {
                // La usuario administrador ya existe => salir
                Console.WriteLine("La usuario administrador ya existe => salir");
                return;
            }


            // Crear el usuario administrador
            var user = new ApplicationUser()
            {
                Nombre = "Admin",
                Apellido = "Admin",
                UserName = "admin@admin.com", // UserName se utilizará para autenticar al usuario
                Email = "admin@admin.com",
                CreadoEn = DateTime.Now,
            };

            string initialPassword = "admin123";


            var result = await userManager.CreateAsync(user, initialPassword);
            if (result.Succeeded)
            {
                // Establecer el rol del usuario
                await userManager.AddToRoleAsync(user, "admin");
                Console.WriteLine("Usuario administrador creado correctamente! Actualice su contraseña inicial!");
                Console.WriteLine("Email: " + user.Email);
                Console.WriteLine("Contraseña inicial: " + initialPassword);
            }
        }

    }
}
