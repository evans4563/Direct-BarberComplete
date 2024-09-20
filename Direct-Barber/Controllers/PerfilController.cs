using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Direct_Barber.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Direct_Barber.Servicios.Contrato;
using Direct_Barber.Recursos;

namespace Direct_Barber.Controllers
{
    public class PerfilController : Controller
    {
        private readonly DirectBarber1Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUsuarioService _usuarioService;

        public PerfilController(DirectBarber1Context context, IWebHostEnvironment webHostEnvironment, IUsuarioService usuarioService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _usuarioService = usuarioService;
        }

        // GET: Perfil
        [Authorize]
        public async Task<IActionResult> Index(Usuario modelo, IFormFile foto)
        {
            // Obtener el correo del usuario autenticado desde los claims
            var correoUsuario = User.FindFirstValue(ClaimTypes.Email);

            // Llamar al servicio para obtener la información del usuario
            var usuario = await _usuarioService.GetUsuario(correoUsuario, null);

            if (usuario == null)
            {
                return NotFound(); // Manejar el caso donde no se encuentra el usuario
            }

            // Retornar la vista con el usuario encontrado
            return View(usuario);
        }



        // GET: Perfil/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Condicional para adaptar la vista según el rol del usuario
            if (usuario.Rol.Nombre == "Barbero")
            {
                // Lógica específica para ver detalles del barbero (incluyendo reseñas, puntuación, etc.)
                ViewBag.EsBarbero = true;
            }
            else
            {
                ViewBag.EsBarbero = false;
            }

            return View(usuario);
        }
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener el usuario por su Id
            var usuario = await _usuarioService.GetUsuarioById(id);

            if (usuario == null)
            {
                return NotFound(); // Si no se encuentra el usuario, se retorna un 404
            }

            return View(usuario); // Enviar el modelo a la vista
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Correo,Direccion,Telefono,Apellido,FecNacimiento")] Usuario usuario, IFormFile foto)
        {
            // Verificar que el ID proporcionado coincida con el del usuario
            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Obtener el usuario original desde la base de datos
            var usuarioExistente = await _usuarioService.GetUsuarioById(id);
            if (usuarioExistente == null)
            {
                return NotFound();
            }

            // Desacoplar la entidad existente para evitar conflictos de rastreo
            _context.Entry(usuarioExistente).State = EntityState.Detached;

            // Mantener los datos que no se están modificando
            usuario.Contrasena = usuarioExistente.Contrasena;
            usuario.Foto = usuarioExistente.Foto;
            usuario.Id_Rol = usuarioExistente.Id_Rol;
            usuario.Documento = usuarioExistente.Documento;
            usuario.FecRegistro = usuarioExistente.FecRegistro;

            // Verificar si se ha subido una nueva imagen
            if (foto != null)
            {
                string uFileName = Utilidades.UploadedFile(foto, _webHostEnvironment); // Usar método centralizado
                if (!string.IsNullOrEmpty(uFileName))
                {
                    usuario.Foto = uFileName; // Asignar el nuevo nombre de archivo
                }
            }
            else
            {
                usuario.Foto = usuarioExistente.Foto; // Mantener la foto original si no hay nueva
            }

            HttpContext.Session.SetString("Foto", usuario.Foto);


            // Remover las validaciones para los campos no editables
            ModelState.Remove("Contrasena");
            ModelState.Remove("Foto");
            ModelState.Remove("Id_Rol");
            ModelState.Remove("Documento");
            ModelState.Remove("Rol");
            ModelState.Remove("ImagenFile");

            if (ModelState.IsValid)
            {
                try
                {
                    // Adjuntar el usuario actualizado y marcarlo como modificado
                    _context.Entry(usuario).State = EntityState.Modified;

                    // Guardar los cambios en la base de datos
                    await _usuarioService.UpdateUsuario(usuario);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _usuarioService.UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // GET: Perfil/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Perfil/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario != null)
            {
                // Eliminar usuario de la base de datos
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                // Limpiar la sesión del usuario
                HttpContext.Session.Clear(); // Eliminar toda la información de la sesión
            }

            // Redirigir al método IniciarSesion del controlador Inicio
            return RedirectToAction("IniciarSesion", "Inicio");
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

    }
}