using CarnesDelSurMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using CarnesDelSurMVC.Services;

namespace CarnesDelSurMVC.Controllers
{
        public class CuentaController : Controller
        {
            private readonly UserManager<ApplicationUser> userManager;
            private readonly SignInManager<ApplicationUser> signInManager;
            private readonly IConfiguration configuration;

            public CuentaController(UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
            {
                this.userManager = userManager;
                this.signInManager = signInManager;
                this.configuration = configuration;
            }


            public IActionResult Register()
            {
                if (signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }


            [HttpPost]
            public async Task<IActionResult> Register(RegisterDto registerDto)
            {
                if (signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                if (!ModelState.IsValid)
                {
                    return View(registerDto);
                }

                // crear una nueva cuenta y autenticar al usuario
                var user = new ApplicationUser()
                {
                    Nombre = registerDto.Nombre,
                    Apellido = registerDto.Apellido,
                    UserName = registerDto.Email, // UserName se utilizará para autenticar al usuario
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.NumTelefono,
                    Direccion = registerDto.Direccion,
                    CreadoEn = DateTime.Now,
                };


                var result = await userManager.CreateAsync(user, registerDto.Contrasena);

                if (result.Succeeded)
                {
                    // Registro de usuario exitoso
                    await userManager.AddToRoleAsync(user, "cliente");

                    // Iniciar sesión con el nuevo usuario
                    await signInManager.SignInAsync(user, false);

                    return RedirectToAction("Index", "Home");
                }


                // Registro fallido => mostrar errores de registro
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(registerDto);
            }


            public async Task<IActionResult> Logout()
            {
                if (signInManager.IsSignedIn(User))
                {
                    await signInManager.SignOutAsync();
                }

                return RedirectToAction("Index", "Home");
            }



            public IActionResult Login()
            {
                if (signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }


            [HttpPost]
            public async Task<IActionResult> Login(LoginDto loginDto)
            {
                if (signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                if (!ModelState.IsValid)
                {
                    return View(loginDto);
                }

                var result = await signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Contrasena,
                    loginDto.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "Intento de inicio de sesión no válido.";
                }

                return View(loginDto);
            }


            [Authorize]
            public async Task<IActionResult> Profile()
            {
                var appUser = await userManager.GetUserAsync(User);
                if (appUser == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var profileDto = new ProfileDto()
                {
                    Nombre = appUser.Nombre,
                    Apellido = appUser.Apellido,
                    Email = appUser.Email ?? "",
                    NumTelefono = appUser.PhoneNumber,
                    Direccion = appUser.Direccion,
                };

                return View(profileDto);
            }


            [Authorize]
            [HttpPost]
            public async Task<IActionResult> Profile(ProfileDto profileDto)
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ErrorMessage = "Por favor, rellene todos los campos obligatorios con valores válidos.";
                    return View(profileDto);
                }

                // Obtener el usuario actual
                var appUser = await userManager.GetUserAsync(User);
                if (appUser == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                // Actualizar el perfil de usuario
                appUser.Nombre = profileDto.Nombre;
                appUser.Apellido = profileDto.Apellido;
                appUser.UserName = profileDto.Email;
                appUser.Email = profileDto.Email;
                appUser.PhoneNumber = profileDto.NumTelefono;
                appUser.Direccion = profileDto.Direccion;

                var result = await userManager.UpdateAsync(appUser);

                if (result.Succeeded)
                {
                    ViewBag.SuccessMessage = "Perfil actualizado exitosamente";
                }
                else
                {
                    ViewBag.ErrorMessage = "No se puede actualizar el perfil: " + result.Errors.First().Description;
                }


                return View(profileDto);
            }


            [Authorize]
            public IActionResult Password()
            {
                return View();
            }


            [Authorize]
            [HttpPost]
            public async Task<IActionResult> Password(PasswordDto passwordDto)
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                // Obtener el usuario actual
                var appUser = await userManager.GetUserAsync(User);
                if (appUser == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                // Actualizar la contraseña
                var result = await userManager.ChangePasswordAsync(appUser,
                    passwordDto.ContrasenaActual, passwordDto.ContrasenaNueva);

                if (result.Succeeded)
                {
                    ViewBag.SuccessMessage = "Contraseña actualizada exitosamente!";
                }
                else
                {
                    ViewBag.ErrorMessage = "Error: " + result.Errors.First().Description;
                }

                return View();
            }



            public IActionResult AccessDenied()
            {
                return RedirectToAction("Index", "Home");
            }


            public IActionResult ForgotPassword()
            {
                if (signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }


            [HttpPost]
            public async Task<IActionResult> ForgotPassword([Required, EmailAddress] string email)
            {
                if (signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Email = email;

                if (!ModelState.IsValid)
                {
                    ViewBag.EmailError = ModelState["email"]?.Errors.First().ErrorMessage ?? "Dirección de correo electrónico no válida";
                    return View();
                }

                var user = await userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    // generar token de restablecimiento de contraseña
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    string resetUrl = Url.ActionLink("ResetPassword", "Cuenta", new { token }) ?? "URL Error";

                    // enviar URL por correo electrónico
                    string senderName = configuration["BrevoSettings:SenderName"] ?? "";
                    string senderEmail = configuration["BrevoSettings:SenderEmail"] ?? "";
                    string username = user.Nombre + " " + user.Apellido;
                    string subject = "Restablecer contraseña";
                    string message = "Querido usuario " + username + ",\n\n" +
									 "Puede restablecer su contraseña utilizando el siguiente enlace:\n\n" +
                                     resetUrl + "\n\n" +
									 "Atentamente Carnes del Sur";

                    EmailSender.SendEmail(senderName, senderEmail, username, email, subject, message);
                }

                ViewBag.SuccessMessage = "¡Por favor revise su cuenta de correo electrónico y haga clic en el enlace de restablecimiento de contraseña!";

                return View();
            }


            public IActionResult ResetPassword(string? token)
            {
                if (signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                if (token == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }


            [HttpPost]
            public async Task<IActionResult> ResetPassword(string? token, PasswordResetDto model)
            {
                if (signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("Index", "Home");
                }

                if (token == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "Token no válido!";
                    return View(model);
                }

                var result = await userManager.ResetPasswordAsync(user, token, model.Contrasena);

                if (result.Succeeded)
                {
                    ViewBag.SuccessMessage = "Restablecimiento de contraseña exitoso!";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View(model);
            }

        }
}
