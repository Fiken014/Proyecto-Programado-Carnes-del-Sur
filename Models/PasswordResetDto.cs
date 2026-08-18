using System.ComponentModel.DataAnnotations;

namespace CarnesDelSurMVC.Models
{
    public class PasswordResetDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MaxLength(100)]
        public string Contrasena { get; set; } = "";

        [Required(ErrorMessage = "El campo Confirmar contraseña es obligatorio")]
        [Compare("Contrasena", ErrorMessage = "Los campos confirmar contraseña y la contraseña no coincide")]
        public string ConfirmContrasena { get; set; } = "";
    }
}
