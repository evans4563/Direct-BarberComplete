using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Direct_Barber.Models;
using System.Security.Claims;

namespace Direct_Barber.Controllers
{
    public class SolicitudsBarberoController : Controller
    {
        private readonly DirectBarber1Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SolicitudsBarberoController(DirectBarber1Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;

        }

        // GET: Solicituds
        public async Task<IActionResult> Index()
        {
            // Obtener el ID del usuario autenticado
            var userIdString = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out var userId))
            {
                // Manejar el caso donde no se pueda obtener el ID del usuario
                return Unauthorized(); // Redirige a una página de error o login
            }

            // Filtrar solicitudes donde IdBarbero esté vacío
            var solicitudesSinBarbero = await _context.Solicituds
                .Include(s => s.IdBarberoNavigation)
                .Include(s => s.IdClienteNavigation)
                .Where(s => s.IdBarbero == null)
                .ToListAsync();

            // Filtrar las solicitudes donde el usuario autenticado sea el barbero
            var solicitudesDelBarbero = await _context.Solicituds
                .Include(s => s.IdBarberoNavigation)
                .Include(s => s.IdClienteNavigation)
                .Where(s => s.IdBarbero == userId)
                .ToListAsync();

            // Crear un ViewModel para enviar ambas listas a la vista
            var viewModel = new SolicitudIndexViewModel
            {
                SolicitudesSinBarbero = solicitudesSinBarbero,
                SolicitudesDelBarbero = solicitudesDelBarbero
            };

            return View(viewModel);
        }

        // GET: Solicituds
        public async Task<IActionResult> Agenda()
        {
            // Obtener el ID del usuario autenticado
            var userIdString = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out var userId))
            {
                // Manejar el caso donde no se pueda obtener el ID del usuario
                return Unauthorized(); // Redirige a una página de error o login
            }

            // Filtrar solicitudes donde IdBarbero esté vacío
            var solicitudesSinBarbero = await _context.Solicituds
                .Include(s => s.IdBarberoNavigation)
                .Include(s => s.IdClienteNavigation)
                .Where(s => s.IdBarbero == null)
                .ToListAsync();

            // Filtrar las solicitudes donde el usuario autenticado sea el barbero
            var solicitudesDelBarbero = await _context.Solicituds
                .Include(s => s.IdBarberoNavigation)
                .Include(s => s.IdClienteNavigation)
                .Where(s => s.IdBarbero == userId)
                .ToListAsync();

            // Crear un ViewModel para enviar ambas listas a la vista
            var viewModel = new SolicitudIndexViewModel
            {
                SolicitudesSinBarbero = solicitudesSinBarbero,
                SolicitudesDelBarbero = solicitudesDelBarbero
            };

            return View(viewModel);
        }





        // GET: Solicituds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitud = await _context.Solicituds
                .Include(s => s.IdBarberoNavigation)
                .Include(s => s.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.IdSolicitud == id);
            if (solicitud == null)
            {
                return NotFound();
            }

            return View(solicitud);
        }

        // GET: Solicituds/Create
        public IActionResult Create()
        {
            ViewData["IdBarbero"] = new SelectList(_context.Usuarios, "Id", "Nombre");
            return View();
        }

        // POST: SolicitudsCliente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSolicitud,IdBarbero,Dirección,Fecha,Descripcion,Precio")] Solicitud solicitud)
        {
            if (ModelState.IsValid)
            {
                // Obtener el ID del usuario autenticado
                var userIdString = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError(string.Empty, "El usuario no está autenticado.");
                    return View(solicitud);
                }

                if (int.TryParse(userIdString, out var userId))
                {
                    solicitud.IdCliente = userId;
                }
                else
                {
                    // Manejar el caso en que el ID del usuario no se puede obtener
                    ModelState.AddModelError(string.Empty, "No se pudo obtener el ID del usuario.");
                    return View(solicitud);
                }

                _context.Add(solicitud);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            ViewData["IdBarbero"] = new SelectList(_context.Usuarios, "Id", "Nombre", solicitud.IdBarbero);
            return View(solicitud);
        }

        // GET: Solicituds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitud = await _context.Solicituds.FindAsync(id);
            if (solicitud == null)
            {
                return NotFound();
            }
            ViewData["IdBarbero"] = new SelectList(_context.Usuarios, "Id", "Contrasena", solicitud.IdBarbero);
            ViewData["IdCliente"] = new SelectList(_context.Usuarios, "Id", "Contrasena", solicitud.IdCliente);
            return View(solicitud);
        }

        // POST: Solicituds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSolicitud,IdCliente,IdBarbero,Dirección,Fecha,Descripcion,Precio")] Solicitud solicitud)
        {
            if (id != solicitud.IdSolicitud)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(solicitud);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SolicitudExists(solicitud.IdSolicitud))
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
            ViewData["IdBarbero"] = new SelectList(_context.Usuarios, "Id", "Contrasena", solicitud.IdBarbero);
            ViewData["IdCliente"] = new SelectList(_context.Usuarios, "Id", "Contrasena", solicitud.IdCliente);
            return View(solicitud);
        }
   

        private bool SolicitudExists(int id)
        {
            return _context.Solicituds.Any(e => e.IdSolicitud == id);
        }


        // POST: Solicituds/Accept
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceptar(int id)
        {
            // Obtener el ID del usuario autenticado
            var userIdString = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized(); // Redirige a una página de error o login
            }

            // Buscar la solicitud con el ID dado
            var solicitud = await _context.Solicituds.FindAsync(id);
            if (solicitud == null)
            {
                return NotFound();
            }

            // Actualizar el campo IdBarbero con el ID del usuario autenticado
            solicitud.IdBarbero = userId;
            _context.Update(solicitud);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Agenda), new { id = solicitud.IdSolicitud });
        }

        // POST: Solicituds/CancelarServicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarServicio(int id)
        {
            // Obtener el ID del usuario autenticado
            var userIdString = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized(); // Redirige a una página de error o login
            }

            // Buscar la solicitud con el ID dado
            var solicitud = await _context.Solicituds.FindAsync(id);
            if (solicitud == null)
            {
                return NotFound();
            }

            // Reiniciar el campo IdBarbero a null
            solicitud.IdBarbero = null;
            _context.Update(solicitud);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Agenda));
        }

        // POST: Solicituds/Completar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Completar(int id)
        {
            // Buscar la solicitud con el ID dado
            var solicitud = await _context.Solicituds.FindAsync(id);

            if (solicitud == null)
            {
                return NotFound(); // Si no existe la solicitud, devolver un error 404.
            }

            // Eliminar la solicitud de la base de datos
            _context.Solicituds.Remove(solicitud);
            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos.

            // Redirigir de vuelta a la vista principal de solicitudes
            return RedirectToAction(nameof(Agenda)); // O Index si prefieres.
        }


    }
}
