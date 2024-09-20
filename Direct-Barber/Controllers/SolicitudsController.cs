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
    public class SolicitudsController : Controller
    {
        private readonly DirectBarber1Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SolicitudsController(DirectBarber1Context context, IHttpContextAccessor httpContextAccessor)
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
                return Unauthorized(); // Redirige a una página de error o login
            }

            // Filtrar solicitudes por cliente autenticado y donde IdBarbero está vacío
            var solicitudesCliente = await _context.Solicituds
                .Include(s => s.IdBarberoNavigation)
                .Include(s => s.IdClienteNavigation)
                .Where(s => s.IdCliente == userId && s.IdBarbero == null)
                .ToListAsync();

            // Filtrar las solicitudes que tienen un barbero asignado
            var solicitudesConBarbero = await _context.Solicituds
                .Include(s => s.IdBarberoNavigation)
                .Include(s => s.IdClienteNavigation)
                .Where(s => s.IdCliente == userId && s.IdBarbero != null)
                .ToListAsync();

            // Crear un ViewModel para enviar ambas listas a la vista
            var viewModel = new SolicitudIndexViewModel
            {
                SolicitudesCliente = solicitudesCliente,
                SolicitudesConBarbero = solicitudesConBarbero
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

        // GET: Solicituds/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Solicituds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitud = await _context.Solicituds.FindAsync(id);
            if (solicitud != null)
            {
                _context.Solicituds.Remove(solicitud);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitudExists(int id)
        {
            return _context.Solicituds.Any(e => e.IdSolicitud == id);
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

            return RedirectToAction(nameof(Index));
        }
    }
}
