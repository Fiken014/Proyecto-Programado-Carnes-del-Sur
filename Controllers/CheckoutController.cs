using CarnesDelSurMVC.Models;
using CarnesDelSurMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json.Nodes;


namespace CarnesDelSurMVC.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private string PaypalClientId { get; set; } = "";
        private string PaypalSecret { get; set; } = "";
        private string PaypalUrl { get; set; } = "";

        private readonly decimal tarifaEnvio;
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public CheckoutController(IConfiguration configuration, ApplicationDbContext context
            , UserManager<ApplicationUser> userManager)
        {
            PaypalClientId = configuration["PaypalSettings:ClientId"]!;
            PaypalSecret = configuration["PaypalSettings:Secret"]!;
            PaypalUrl = configuration["PaypalSettings:Url"]!;

            tarifaEnvio = configuration.GetValue<decimal>("CartSettings:TarifaEnvio");
            this.context = context;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            List<OrdenItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal total = Math.Round((CartHelper.GetSubtotal(cartItems) + tarifaEnvio + (CartHelper.GetSubtotal(cartItems) * 0.13m)) / 500, 2);

            string direccionEnvio = TempData["DireccionEnvio"] as string ?? "";
            TempData.Keep();

            ViewBag.DireccionEnvio = direccionEnvio;
            ViewBag.Total = total;
            ViewBag.PaypalClientId = PaypalClientId;
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> CreateOrder()
        {
            List<OrdenItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal totalAmount = Math.Round((CartHelper.GetSubtotal(cartItems) + tarifaEnvio + (CartHelper.GetSubtotal(cartItems) * 0.13m))/500, 2);



            // Crear el cuerpo de la solicitud
            JsonObject createOrderRequest = new JsonObject();
            createOrderRequest.Add("intent", "CAPTURE");

            JsonObject amount = new JsonObject();
            amount.Add("currency_code", "USD");
            amount.Add("value", totalAmount);

            JsonObject purchaseUnit1 = new JsonObject();
            purchaseUnit1.Add("amount", amount);

            JsonArray purchaseUnits = new JsonArray();
            purchaseUnits.Add(purchaseUnit1);

            createOrderRequest.Add("purchase_units", purchaseUnits);


            // Obtener token de acceso
            string accessToken = await GetPaypalAccessToken();

            // Enviar solicitud
            string url = PaypalUrl + "/v2/checkout/orders";


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse != null)
                    {
                        string paypalOrderId = jsonResponse["id"]?.ToString() ?? "";

                        return new JsonResult(new { Id = paypalOrderId });
                    }
                }
            }


            return new JsonResult(new { Id = "" });
        }


        [HttpPost]
        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
        {
            var ordenId = data?["ordenId"]?.ToString();
            var direccionEnvio = data?["direccionEnvio"]?.ToString();

            if (ordenId == null || direccionEnvio == null)
            {
                return new JsonResult("error");
            }

            // Obtener token de acceso
            string accessToken = await GetPaypalAccessToken();


            string url = PaypalUrl + "/v2/checkout/orders/" + ordenId + "/capture";


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("", null, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse != null)
                    {
                        string paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";
                        if (paypalOrderStatus == "COMPLETED")
                        {
                            // Guardar el pedido en la base de datos
                            await SaveOrder(jsonResponse.ToString(), direccionEnvio);

                            return new JsonResult("success");
                        }
                    }
                }
            }


            return new JsonResult("error");
        }

        private async Task SaveOrder(string paypalResponse, string deliveryAddress)
        {
            // Obtener artículos del carrito
            var cartItems = CartHelper.GetCartItems(Request, Response, context);

            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return;
            }

            // Guardar el pedido
            var orden = new Orden
            {
                ClienteId = appUser.Id,
                Items = cartItems,
                TarifaEnvio = tarifaEnvio,
                DireccionEntrega = deliveryAddress,
                MetodoPago = "paypal",
                EstadoPago = "aceptado",
                DetallePago = paypalResponse,
                OrdenStatus = "pendiente",
                CreadoEn = DateTime.Now,
            };

            context.Ordenes.Add(orden);
            context.SaveChanges();


            // Eliminar la cookie del carrito de compras
            Response.Cookies.Delete("shopping_cart");
        }



        /*
        public async Task<string> Token()
        {
            return await GetPaypalAccessToken();
        }
        */

        private async Task<string> GetPaypalAccessToken()
        {
            string accessToken = "";


            string url = PaypalUrl + "/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                string credentials64 =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(PaypalClientId + ":" + PaypalSecret));

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials64);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("grant_type=client_credentials", null
                    , "application/x-www-form-urlencoded");

                var httpResponse = await client.SendAsync(requestMessage);


                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();

                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                    }
                }
            }


            return accessToken;
        }
    }
}
