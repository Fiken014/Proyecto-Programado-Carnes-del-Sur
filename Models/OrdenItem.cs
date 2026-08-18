using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarnesDelSurMVC.Models
{
    [Table("OrdenItems")]
    public class OrdenItem
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }

        [Precision(16, 2)]
        public decimal PrecioUnidad { get; set; }

        // propiedad de navegación
        public Producto Producto { get; set; } = new Producto();
    }
}
