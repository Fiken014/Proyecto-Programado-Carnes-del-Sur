using CarnesDelSurMVC.Models;
using CarnesDelSurMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CarnesDelSurMVC.Controllers
{
    [Authorize(Roles = "cliente")]
    [Route("/Cliente/Ordenes/{action=Index}/{id?}")]
    public class ClienteOrdenesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly int pageSize = 5;

        public ClienteOrdenesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageIndex)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            IQueryable<Orden> query = context.Ordenes
                .Include(o => o.Items).OrderByDescending(o => o.Id)
                .Where(o => o.ClienteId == currentUser.Id);

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


        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var orden = context.Ordenes.Include(o => o.Items)
                .ThenInclude(oi => oi.Producto)
                .Where(o => o.ClienteId == currentUser.Id).FirstOrDefault(o => o.Id == id);


            if (orden == null)
            {
                return RedirectToAction("Index");
            }


            return View(orden);
        }
    }
}
