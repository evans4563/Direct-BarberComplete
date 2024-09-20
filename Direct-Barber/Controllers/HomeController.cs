using Direct_Barber.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Direct_Barber.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //Mostrar el nombre del usuario
            ClaimsPrincipal claimuser = HttpContext.User;
            string nombreUsuario = "";


            //Si el usuario existe
            if (claimuser.Identity.IsAuthenticated)
            {
                //Pasa a mostrar el nombre
                nombreUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.Name)
                    .Select(c => c.Value).SingleOrDefault();
            }

            ViewData["NombreUsuario"] = nombreUsuario;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Barbero()
        {
            return View();
        }

        public IActionResult Cliente()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> CerrarSesion()
        {
            //Borrar la autenticacion con SignOutAsync.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); 
            return RedirectToAction("IniciarSesion", "Inicio");
        }
    }
}
