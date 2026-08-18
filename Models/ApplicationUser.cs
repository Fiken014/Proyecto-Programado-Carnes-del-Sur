using Microsoft.AspNetCore.Identity;

namespace CarnesDelSurMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Direccion { get; set; } = "";
        public DateTime CreadoEn { get; set; }
    }
}
