using System.ComponentModel.DataAnnotations;

namespace CarnesDelSurMVC.Models
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El campo Nombre es obligatorio"), MaxLength(100)]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El campo Apellido es obligatorio"), MaxLength(100)]
        public string Apellido { get; set; } = "";

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";

        [Phone(ErrorMessage = "El formato del número de teléfono no es válido"), MaxLength(20)]
        public string? NumTelefono { get; set; }

        [Required, MaxLength(200)]
        public string Direccion { get; set; } = "";

        [Required, MaxLength(100)]
        public string Contrasena { get; set; } = "";

        [Required(ErrorMessage = "El campo Confirmar contraseña es obligatorio")]
        [Compare("Contrasena", ErrorMessage = "Los campos confirmar contraseña y la contraseña no coincide")]
        public string ConfirmContrasena { get; set; } = "";
    }
}
