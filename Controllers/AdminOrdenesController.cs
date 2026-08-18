using CarnesDelSurMVC.Models;
using CarnesDelSurMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


namespace CarnesDelSurMVC.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/Ordenes/{action=Index}/{id?}")]
    public class AdminOrdenesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 5;

        public AdminOrdenesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index(int pageIndex)
        {
            IQueryable<Orden> query = context.Ordenes.Include(o => o.Cliente)
                .Include(o => o.Items).OrderByDescending(o => o.Id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }


            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);

            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);


            var ordenes = query.ToList();

            ViewBag.Ordenes = ordenes;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View();
        }


        public IActionResult Details(int id)
        {
            var order = context.Ordenes.Include(o => o.Cliente).Include(o => o.Items)
                .ThenInclude(oi => oi.Producto).FirstOrDefault(o => o.Id == id);


            if (order == null)
            {
                return RedirectToAction("Index");
            }


            ViewBag.NumOrdenes = context.Ordenes.Where(o => o.ClienteId == order.ClienteId).Count();

            return View(order);
        }


        public IActionResult Edit(int id, string? estado_pago, string? orden_status)
        {
            var order = context.Ordenes.Find(id);
            if (order == null)
            {
                return RedirectToAction("Index");
            }


            if (estado_pago == null && orden_status == null)
            {
                return RedirectToAction("Details", new { id });
            }

            if (estado_pago != null)
            {
                order.EstadoPago = estado_pago;
            }

            if (orden_status != null)
            {
                order.OrdenStatus = orden_status;
            }

            context.SaveChanges();


            return RedirectToAction("Details", new { id });
        }

        public IActionResult ProductosMasVendidos()
        {
            var productosMasVendidos = context.Ordenes
                .SelectMany(o => o.Items)
                .GroupBy(oi => oi.Producto.Nombre)
                .Select(g => new ProductoMasVendido
                {
                    Producto = g.Key,
                    CantidadVendida = g.Sum(oi => oi.Cantidad)
                })
                .OrderByDescending(g => g.CantidadVendida)
                .Take(10)
                .ToList();

            return View(productosMasVendidos); 
        }

        public IActionResult VentasPorMes()
        {
            var ventas = context.Ordenes
                .Include(o => o.Items) // Asegúrate de incluir los items para poder calcular el total
                .ToList() // Se traen todos los datos de la tabla y pasa a memoria
                .GroupBy(o => new { o.CreadoEn.Year, o.CreadoEn.Month })
                .Select(g => new VentasPorMes
                {
                    Mes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month) + " " + g.Key.Year,
                    TotalVentas = g.Sum(o =>
                        o.Items.Sum(i => i.PrecioUnidad * i.Cantidad) + o.TarifaEnvio
                    )
                })
                .OrderByDescending(v => v.Mes)
                .ToList();

            return View(ventas);
        }






    }
}
