using System.ComponentModel.DataAnnotations;

namespace CarnesDelSurMVC.Models
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Contrasena { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
