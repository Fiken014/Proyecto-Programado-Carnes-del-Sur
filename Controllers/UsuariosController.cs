using CarnesDelSurMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarnesDelSurMVC.Controllers
{
	[Authorize(Roles = "admin")]
	[Route("/Admin/[controller]/{action=Index}/{id?}")]
	public class UsuariosController : Controller
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
        private readonly int pageSize = 5;

        public UsuariosController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.roleManager = roleManager;
		}

		public IActionResult Index(int? pageIndex)
		{
			IQueryable<ApplicationUser> query = userManager.Users.OrderByDescending(u => u.CreadoEn);

			// funcionalidad de paginación
			if (pageIndex == null || pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip(((int)pageIndex - 1) * pageSize).Take(pageSize);

            var users = query.ToList();

            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View(users);
		}


		public async Task<IActionResult> Details(string? id)
		{
            if (id == null)
            {
                return RedirectToAction("Index", "Usuarios");
            }

            var appUser = await userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Usuarios");
            }

            ViewBag.Roles = await userManager.GetRolesAsync(appUser);

			// get available roles
			var availableRoles = roleManager.Roles.ToList();
			var items = new List<SelectListItem>();
			foreach (var role in availableRoles)
			{
				items.Add(
					new SelectListItem
					{
						Text = role.NormalizedName,
						Value = role.Name,
						Selected = await userManager.IsInRoleAsync(appUser, role.Name!),
					});
			}

			ViewBag.SelectItems = items;

			return View(appUser);
		}


		public async Task<IActionResult> EditRole(string? id, string? newRole)
		{
			if (id == null || newRole == null)
			{
				return RedirectToAction("Index", "Usuarios");
			}

			var roleExists = await roleManager.RoleExistsAsync(newRole);
			var appUser = await userManager.FindByIdAsync(id);

			if (appUser == null || !roleExists)
			{
				return RedirectToAction("Index", "Usuarios");
			}

			var currentUser = await userManager.GetUserAsync(User);
			if (currentUser!.Id == appUser.Id)
			{
				TempData["ErrorMessage"] = "No puedes actualizar tu propio rol!";
				return RedirectToAction("Details", "Usuarios", new { id });
			}

			// update user role
			var userRoles = await userManager.GetRolesAsync(appUser);
			await userManager.RemoveFromRolesAsync(appUser, userRoles);
			await userManager.AddToRoleAsync(appUser, newRole);

			TempData["SuccessMessage"] = "Rol de usuario actualizado exitosamente";
			return RedirectToAction("Details", "Usuarios", new { id });
		}



		public async Task<IActionResult> DeleteAccount(string? id)
		{
			if (id == null)
			{
				return RedirectToAction("Index", "Usuarios");
			}

			var appUser = await userManager.FindByIdAsync(id);

			if (appUser == null)
			{
				return RedirectToAction("Index", "Usuarios");
			}

			var currentUser = await userManager.GetUserAsync(User);
			if (currentUser!.Id == appUser.Id)
			{
				TempData["ErrorMessage"] = "No puedes eliminar tu propia cuenta!";
				return RedirectToAction("Details", "Usuarios", new { id });
			}

			// delete user account
			var result = await userManager.DeleteAsync(appUser);
			if (result.Succeeded)
			{
				return RedirectToAction("Index", "Usuarios");
			}

			TempData["ErrorMessage"] = "No se puede eliminar esta cuenta: " + result.Errors.First().Description;
			return RedirectToAction("Details", "Usuarios", new { id });
		}
	}
}
