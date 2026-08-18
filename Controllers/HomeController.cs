using CarnesDelSurMVC.Models;
using CarnesDelSurMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CarnesDelSurMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;

        public HomeController(ApplicationDbContext context)
        {
           this.context = context;
        }

        public IActionResult Index()
        {
            var productos = context.Productos.OrderByDescending(p => p.Id).Take(4).ToList();
            return View(productos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}