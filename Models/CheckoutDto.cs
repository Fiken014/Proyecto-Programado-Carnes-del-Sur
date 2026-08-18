using System.ComponentModel.DataAnnotations;

namespace CarnesDelSurMVC.Models
{
    public class CheckoutDto
    {
        [Required(ErrorMessage = "La dirección de entrega es obligatoria.")]
        [MaxLength(200)]
        public string DireccionEntrega { get; set; } = "";
        public string MetodoPago { get; set; } = "";
    }
}
