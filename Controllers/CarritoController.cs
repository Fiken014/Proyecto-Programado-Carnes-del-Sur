using CarnesDelSurMVC.Models;
using CarnesDelSurMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarnesDelSurMVC.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly decimal tarifaEnvio;

        public CarritoController(ApplicationDbContext context, IConfiguration configuration
            , UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
            tarifaEnvio = configuration.GetValue<decimal>("CartSettings:TarifaEnvio");
        }

        public IActionResult Index()
        {
            List<OrdenItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);


            ViewBag.CartItems = cartItems;
            ViewBag.TarifaEnvio = tarifaEnvio;
            ViewBag.Subtotal = subtotal;
            ViewBag.Total = Math.Round(subtotal + (subtotal * 0.13m) + tarifaEnvio, 2);

            return View();
        }


        [Authorize]
        [HttpPost]
        public IActionResult Index(CheckoutDto model)
        {
            List<OrdenItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);

            // Ajusta la tarifa de envío en función de la selección del usuario
            decimal tarifaEnvioAplicada = model.DireccionEntrega == "Retiro en el local" ? 0 : tarifaEnvio;

            ViewBag.CartItems = cartItems;
            ViewBag.TarifaEnvio = tarifaEnvioAplicada;
            ViewBag.Subtotal = subtotal;
            ViewBag.Total = Math.Round(subtotal + (subtotal * 0.13m) + tarifaEnvioAplicada, 2);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Comprueba si el carrito de compras está vacío o no
            if (cartItems.Count == 0)
            {
                ViewBag.ErrorMessage = "Tu carrito esta vacío";
                return View(model);
            }


            TempData["DireccionEntrega"] = model.DireccionEntrega;
            TempData["MetodoPago"] = model.MetodoPago;
            TempData["TarifaEnvio"] = tarifaEnvioAplicada.ToString();


            if (model.MetodoPago == "paypal" || model.MetodoPago == "credit_card")
            {
                return RedirectToAction("Index", "Checkout");
            }

            return RedirectToAction("Confirm");
        }



        public IActionResult Confirm()
        {
            List<OrdenItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);

            decimal tarifaEnvioAplicada = TempData["TarifaEnvio"] != null ? decimal.Parse(TempData["TarifaEnvio"].ToString()) : tarifaEnvio;

            decimal total = Math.Round(subtotal + tarifaEnvioAplicada + (subtotal * 0.13m), 2);

            string direccionEntrega = TempData["DireccionEntrega"] as string ?? "";
            string metodoPago = TempData["MetodoPago"] as string ?? "";

            int cartSize = 0;
            foreach (var item in cartItems)
            {
                cartSize += item.Cantidad;
            }

            TempData.Keep();


            if (cartSize == 0 || direccionEntrega.Length == 0 || metodoPago.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.DireccionEntrega = direccionEntrega;
            ViewBag.MetodoPago = metodoPago;
            ViewBag.Total = total;
            ViewBag.CartSize = cartSize;
            ViewBag.TarifaEnvio = tarifaEnvioAplicada;

            return View();
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Confirm(int any)
        {
            var cartItems = CartHelper.GetCartItems(Request, Response, context);

            string direccionEntrega = TempData["DireccionEntrega"] as string ?? "";
            string metodoPago = TempData["MetodoPago"] as string ?? "";
            decimal tarifaEnvioAplicada = TempData["TarifaEnvio"] != null ? decimal.Parse(TempData["TarifaEnvio"].ToString()) : tarifaEnvio;

            TempData.Keep();

            if (cartItems.Count == 0 || direccionEntrega.Length == 0 || metodoPago.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Guardar el pedido
            var order = new Orden
            {
                ClienteId = appUser.Id,
                Items = cartItems,
                TarifaEnvio = tarifaEnvioAplicada,
                DireccionEntrega = direccionEntrega,
                MetodoPago = metodoPago,
                EstadoPago = "pendiente",
                DetallePago = "",
                OrdenStatus = "creada",
                CreadoEn = DateTime.Now,
            };

            context.Ordenes.Add(order);
            context.SaveChanges();


            // Eliminar la cookie del carrito de compras
            Response.Cookies.Delete("shopping_cart");

            ViewBag.SuccessMessage = "Pedido creado exitosamente";

            return View();
        }
    }
}
