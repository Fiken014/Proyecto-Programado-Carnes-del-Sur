using System.Text.RegularExpressions;
using CarnesDelSurMVC.Models;
using CarnesDelSurMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarnesDelSurMVC.Controllers
{
    public class TiendaController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 8;

        public TiendaController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index(int pageIndex, string? search, string? marca, string? categoria, string? sort)
        {
            IQueryable<Producto> query = context.Productos;

            // funcionalidad de búsqueda
            if (search != null && search.Length > 0)
            {
                query = query.Where(p => p.Nombre.Contains(search));
            }


            // funcionalidad de filtrado
            if (marca != null && marca.Length > 0)
            {
                query = query.Where(p => p.Marca.Contains(marca));
            }

            if (categoria != null && categoria.Length > 0)
            {
                query = query.Where(p => p.Categoria.Contains(categoria));
            }

            // funcionalidad de ordenar
            if (sort == "price_asc")
            {
                query = query.OrderBy(p => p.Precio);
            }
            else if (sort == "price_desc")
            {
                query = query.OrderByDescending(p => p.Precio);
            }
            else
            {
                // Los productos más nuevos primero
                query = query.OrderByDescending(p => p.Id);
            }



            // funcionalidad de paginación
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);


            var productos = query.ToList();

            ViewBag.Productos = productos;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            var storeSearchModel = new StoreSearchModel()
            {
                Search = search,
                Marca = marca,
                Categoria = categoria,
                Sort = sort
            };

            return View(storeSearchModel);
        }


        public IActionResult Details(int id)
        {
            var producto = context.Productos.Find(id);
            if (producto == null)
            {
                return RedirectToAction("Index", "Tienda");
            }

            return View(producto);
        }
    }
}
