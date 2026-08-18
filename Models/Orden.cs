using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CarnesDelSurMVC.Models
{
    public class Orden
    {
        public int Id { get; set; }

        public string ClienteId { get; set; } = "";
        public ApplicationUser Cliente { get; set; } = null!;

        public List<OrdenItem> Items { get; set; } = new List<OrdenItem>();

        [Precision(16, 2)]
        public decimal TarifaEnvio { get; set; }

        public string DireccionEntrega { get; set; } = "";
        public string MetodoPago { get; set; } = "";
        public string EstadoPago { get; set; } = "";
        public string DetallePago { get; set; } = "";
        public string OrdenStatus { get; set; } = "";
        public DateTime CreadoEn { get; set; }
        [NotMapped]
        public decimal Total { get; set; }
    }
}
