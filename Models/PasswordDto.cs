using System.ComponentModel.DataAnnotations;

namespace CarnesDelSurMVC.Models
{
	public class PasswordDto
	{
		[Required(ErrorMessage = "El campo Contraseña actual es obligatorio"), MaxLength(100)]
		public string ContrasenaActual { get; set; } = "";

		[Required(ErrorMessage = "El campo Nueva contraseña es obligatorio"), MaxLength(100)]
		public string ContrasenaNueva { get; set; } = "";

		[Required(ErrorMessage = "El campo Confirmar contraseña es obligatorio")]
		[Compare("ContrasenaNueva", ErrorMessage = "Los campos confirmar contraseña y la contraseña no coincide")]
		public string ConfirmContrasena { get; set; } = "";
	}
}
