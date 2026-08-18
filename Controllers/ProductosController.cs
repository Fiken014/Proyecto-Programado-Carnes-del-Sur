using CarnesDelSurMVC.Models;
using CarnesDelSurMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarnesDelSurMVC.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly int pageSize = 5;

        public ProductosController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index(int pageIndex, string? search, string? column, string? orderBy)
        {
            IQueryable<Producto> query = context.Productos;

            // funcionalidad de búsqueda
            if (search != null)
            {
                query = query.Where(p => p.Nombre.Contains(search) || p.Marca.Contains(search));
            }

            // funcionalidad de clasificación
            string[] validColumns = { "Id", "Nombre", "Marca", "Categoria", "Precio", "CreadoEn" };
            string[] validOrderBy = { "desc", "asc" };

            if (!validColumns.Contains(column))
            {
                column = "Id";
            }

            if (!validOrderBy.Contains(orderBy))
            {
                orderBy = "desc";
            }

            if (column == "Nombre")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Nombre);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Nombre);
                }
            }
            else if (column == "Marca")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Marca);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Marca);
                }
            }
            else if (column == "Categoria")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Categoria);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Categoria);
                }
            }
            else if (column == "Precio")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Precio);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Precio);
                }
            }
            else if (column == "CreadoEn")
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.CreadoEn);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreadoEn);
                }
            }
            else
            {
                if (orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Id);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
            }

            //funcionalidad de paginación
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var productos = query.ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;

            ViewData["Search"] = search ?? "";

            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;

            return View(productos);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(ProductoDto productoDto)
        {
            if (productoDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "El archivo de imagen es obligatorio");
            }

            if (!ModelState.IsValid)
            {
                return View(productoDto);
            }


            // guardar el archivo de imagen
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productoDto.ImageFile!.FileName);

            string imageFullPath = environment.WebRootPath + "/productos/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productoDto.ImageFile.CopyTo(stream);
            }

            // guardar el nuevo producto en la base de datos
            Producto product = new Producto()
            {
                Nombre = productoDto.Nombre,
                Marca = productoDto.Marca,
                Categoria = productoDto.Categoria,
                Precio = productoDto.Precio,
                Descripcion = productoDto.Descripcion,
                ImageFileName = newFileName,
                CreadoEn = DateTime.Now,
            };


            context.Productos.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Productos");
        }


        public IActionResult Edit(int id)
        {
            var producto = context.Productos.Find(id);

            if (producto == null)
            {
                return RedirectToAction("Index", "Productos");
            }

            // crear productoDto a partir de producto
            var productoDto = new ProductoDto()
            {
                Nombre = producto.Nombre,
                Marca = producto.Marca,
                Categoria = producto.Categoria,
                Precio = producto.Precio,
                Descripcion = producto.Descripcion,
            };


            ViewData["ProductoId"] = producto.Id;
            ViewData["ImageFileName"] = producto.ImageFileName;
            ViewData["CreadoEn"] = producto.CreadoEn.ToString("MM/dd/yyyy");

            return View(productoDto);
        }


        [HttpPost]
        public IActionResult Edit(int id, ProductoDto productoDto)
        {
            var producto = context.Productos.Find(id);

            if (producto == null)
            {
                return RedirectToAction("Index", "Productos");
            }


            if (!ModelState.IsValid)
            {
                ViewData["ProductoId"] = producto.Id;
                ViewData["ImageFileName"] = producto.ImageFileName;
                ViewData["CreadoEn"] = producto.CreadoEn.ToString("MM/dd/yyyy");

                return View(productoDto);
            }


            // Actualizar el archivo de imagen si tenemos un nuevo archivo de imagen
            string newFileName = producto.ImageFileName;
            if (productoDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productoDto.ImageFile.FileName);

                string imageFullPath = environment.WebRootPath + "/productos/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productoDto.ImageFile.CopyTo(stream);
                }

                // eliminar la imagen antigua
                string oldImageFullPath = environment.WebRootPath + "/productos/" + producto.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }


            // Actualizar el producto en la base de datos
            producto.Nombre = productoDto.Nombre;
            producto.Marca = productoDto.Marca;
            producto.Categoria = productoDto.Categoria;
            producto.Precio = productoDto.Precio;
            producto.Descripcion = productoDto.Descripcion;
            producto.ImageFileName = newFileName;


            context.SaveChanges();

            return RedirectToAction("Index", "Productos");
        }

        public IActionResult Delete(int id)
        {
            var producto = context.Productos.Find(id);
            if (producto == null)
            {
                return RedirectToAction("Index", "Productos");
            }

            string imageFullPath = environment.WebRootPath + "/productos/" + producto.ImageFileName;
            System.IO.File.Delete(imageFullPath);

            context.Productos.Remove(producto);
            context.SaveChanges(true);

            return RedirectToAction("Index", "Productos");
        }
    }
}
