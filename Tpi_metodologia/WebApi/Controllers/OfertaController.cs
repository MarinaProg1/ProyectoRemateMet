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
        private readonly RemateService _remateService;

        public OfertaController(SubastaMetodologiaDbContext context, RemateService remateService)
        {
            _context = context;
            _remateService = remateService;
        }

        [HttpPost("ofertar")]
        [Authorize]
        public async Task<IActionResult> RealizarOferta([FromBody] OfertaDto dto)
        {
            try
            {
                // Obtener el producto desde la base de datos usando el IdProducto del cuerpo
                var producto = await _context.Productos.FindAsync(dto.IdProducto);
                if (producto == null)
                    return NotFound("Producto no encontrado");

                // Verificar si el producto está aprobado
                if (producto.Estado != "aprobado")
                    return BadRequest("El producto aún no está aprobado para recibir ofertas.");

                // Verificar que el monto de la oferta sea mayor al precio base
                if (dto.Monto <= producto.PrecioBase)
                    return BadRequest($"El monto de la oferta debe ser mayor al precio base, que es {producto.PrecioBase:C}.");

                // Verificar si ya se ha realizado una oferta para este producto por el mismo usuario
                var ofertaExistente = await _context.Ofertas
                    .FirstOrDefaultAsync(o => o.IdUsuario == dto.IdUsuario && o.IdProducto == dto.IdProducto);
                if (ofertaExistente != null)
                    return BadRequest("Ya has realizado una oferta para este producto.");

                // Crear la nueva oferta
                var oferta = new Oferta
                {
                    IdUsuario = dto.IdUsuario,
                    IdProducto = dto.IdProducto,
                    Monto = dto.Monto,
                    Fecha = DateTime.Now,
                    Estado = "pendiente"
                };

                // Agregar la oferta a la base de datos
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
                ofertaGanadora.Estado = "ganadora";

                // Crear la factura
                var factura = new Factura
                {
                    IdOferta = ofertaGanadora.IdOferta,
                    Fecha = DateTime.Now,
                    Monto = ofertaGanadora.Monto
                };

                _context.Facturas.Add(factura);
                _context.Ofertas.Update(ofertaGanadora);
                await _context.SaveChangesAsync();

                // Obtener información del usuario ganador
                var usuarioGanador = await _context.Usuarios.FindAsync(ofertaGanadora.IdUsuario);
                if (usuarioGanador != null && !string.IsNullOrEmpty(usuarioGanador.Email))
                {
                    _remateService.EnviarCorreoFactura(usuarioGanador.Email, usuarioGanador.Nombre, factura);
                }

                return Ok(new { mensaje = "Oferta ganadora seleccionada y factura generada", factura });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}

