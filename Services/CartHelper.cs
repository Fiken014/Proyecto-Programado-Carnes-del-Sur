using CarnesDelSurMVC.Models;
using System.Text.Json;

namespace CarnesDelSurMVC.Services
{
    public class CartHelper
    {
        public static Dictionary<int, int> GetCartDictionary(HttpRequest request, HttpResponse response)
        {
            string cookieValue = request.Cookies["shopping_cart"] ?? "";

            try
            {
                var cart = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));
                Console.WriteLine("[CartHelper] cart=" + cookieValue + " -> " + cart);
                var dictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);
                if (dictionary != null)
                {
                    return dictionary;
                }
            }
            catch(Exception)
            {
            }

            if (cookieValue.Length > 0)
            {
                // Esta cookie no es válida => elimínela
                response.Cookies.Delete("shopping_cart");
            }

            return new Dictionary<int, int>();
        }


        public static int GetCartSize(HttpRequest request, HttpResponse response)
        {
            int cartSize = 0;

            var cartDictionary = GetCartDictionary(request, response);
            foreach (var keyValuePair in cartDictionary)
            {
                cartSize += keyValuePair.Value;
            }

            return cartSize;
        }

        public static List<OrdenItem> GetCartItems(HttpRequest request, HttpResponse response
            , ApplicationDbContext context)
        {
            var cartItems = new List<OrdenItem>();

            var cartDictionary = GetCartDictionary(request, response);
            foreach (var pair in cartDictionary)
            {
                int productId = pair.Key;
                int quantity = pair.Value;
                var product = context.Productos.Find(productId);
                if (product == null) continue;

                var item = new OrdenItem
                {
                    Cantidad = quantity,
                    PrecioUnidad = product.Precio,
                    Producto = product,
                };

                cartItems.Add(item);
            }

            return cartItems;
        }


        public static decimal GetSubtotal(List<OrdenItem> cartItems)
        {
            decimal subtotal = 0;

            foreach (var item in cartItems)
            {
                subtotal += item.Cantidad * item.PrecioUnidad;
            }

            return subtotal;
        }
    }
}
