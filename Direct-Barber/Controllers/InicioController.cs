using Microsoft.AspNetCore.Mvc;
using Direct_Barber.Models;
using Direct_Barber.Recursos;
using Direct_Barber.Servicios.Contrato;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Direct_Barber.Controllers
{
    public class InicioController : Controller
    {
        private readonly IUsuarioService _usuarioServicio;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InicioController(IUsuarioService usuarioServicio, IWebHostEnvironment webHostEnvironment)
        {
            _usuarioServicio = usuarioServicio;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Registrarse()
        {
            var roles = await _usuarioServicio.GetRoles();
            ViewBag.Roles = new SelectList(roles, "Id", "Nombre");
            ViewData["Mensaje"] = null;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(Usuario modelo, IFormFile foto)
        {
            if (foto == null || foto.Length == 0)
            {
                ViewData["Mensaje"] = "No se ha seleccionado una imagen o la imagen está vacía.";
                return View(modelo);
            }

            string uFileName = Utilidades.UploadedFile(foto, _webHostEnvironment);
            modelo.Foto = uFileName;
            modelo.Contrasena = Utilidades.EncriptarContra(modelo.Contrasena);
            Usuario usuario_creado = await _usuarioServicio.SaveUsuario(modelo);

            if (usuario_creado.Id > 0)
                return RedirectToAction("IniciarSesion", "Inicio");

            ViewData["Mensaje"] = "No se pudo crear el usuario";
            return View();
        }

        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion(string Correo, string Contrasena)
        {
            Usuario usuario_encontrado = await _usuarioServicio.GetUsuario(Correo, Utilidades.EncriptarContra(Contrasena));

            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "No se encontraron coincidencias";
                return View();
            }

            HttpContext.Session.SetString("Foto", usuario_encontrado.Foto ?? "usuario.png");

            // **Aquí se añade el `Id` del cliente a los reclamos**
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario_encontrado.Nombre),
                new Claim(ClaimTypes.Role, usuario_encontrado.Rol.Nombre),
                new Claim(ClaimTypes.Email, usuario_encontrado.Correo),
                new Claim(ClaimTypes.NameIdentifier, usuario_encontrado.Id.ToString()) // Agregar el Id del cliente como reclamo
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties properties = new AuthenticationProperties
            {
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
            );

            if (usuario_encontrado.Rol.Nombre == "Barbero")
            {
                return RedirectToAction("Barbero", "Home");
            }
            else if (usuario_encontrado.Rol.Nombre == "Cliente")
            {
                return RedirectToAction("Cliente", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
