using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Models.DTOs.Oferta;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfertaController : ControllerBase
    {
        private readonly SubastaMetodologiaDbContext _context;

        public OfertaController(SubastaMetodologiaDbContext context)
        {
            _context = context;
        }

        [HttpPost("ofertar/{idProducto}")]
        [Authorize]
        public async Task<IActionResult> RealizarOferta(int idProducto, [FromBody] OfertaDto dto)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(idProducto);
                if (producto == null) return NotFound("Producto no encontrado");

                if (dto.Monto <= producto.PrecioBase)
                    return BadRequest("El monto de la oferta debe ser mayor al precio base.");

                var ofertaExistente = await _context.Ofertas
                    .FirstOrDefaultAsync(o => o.IdUsuario == dto.IdUsuario && o.IdProducto == idProducto);
                if (ofertaExistente != null)
                    return BadRequest("Ya has realizado una oferta para este producto.");

                var oferta = new Oferta
                {
                    IdUsuario = dto.IdUsuario,
                    IdProducto = idProducto,
                    Monto = dto.Monto,
                    Fecha = DateTime.Now,
                    Estado = "Pendiente"
                };

                _context.Ofertas.Add(oferta);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Oferta realizada exitosamente", idOferta = oferta.IdOferta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        
        [HttpGet("ofertas/{idProducto}")]
        [Authorize]
        public async Task<IActionResult> ObtenerOfertas(int idProducto)
        {
            try
            {
                var ofertas = await _context.Ofertas
                    .Where(o => o.IdProducto == idProducto)
                    .OrderByDescending(o => o.Monto)
                    .ThenBy(o => o.Fecha)
                    .ToListAsync();

                if (ofertas == null || ofertas.Count == 0)
                    return NotFound("No hay ofertas para este producto.");

                return Ok(ofertas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost("seleccionar-ganadora/{idProducto}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeleccionarOfertaGanadora(int idProducto)
        {
            try
            {
                var ofertas = await _context.Ofertas
                    .Where(o => o.IdProducto == idProducto)
                    .OrderByDescending(o => o.Monto)
                    .ThenBy(o => o.Fecha)
                    .ToListAsync();

                if (ofertas == null || ofertas.Count == 0)
                    return NotFound("No hay ofertas para este producto.");

                var ofertaGanadora = ofertas.First();
                ofertaGanadora.Estado = "Ganadora";

                _context.Ofertas.UpdateRange(ofertas);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Oferta ganadora seleccionada", ofertaGanadora });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}
