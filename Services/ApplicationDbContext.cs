using CarnesDelSurMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CarnesDelSurMVC.Services
{
    public class ApplicationDbContext : IdentityDbContext <ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet <Producto> Productos { get; set; }
        public DbSet<Orden> Ordenes { get; set; }

    }
}
