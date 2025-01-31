
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Models.DTOs.Producto;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly SubastaMetodologiaDbContext _context;

        public ProductoController(SubastaMetodologiaDbContext context)
        {
            _context = context;
        }

        [HttpPost("publicar")]
        [Authorize]
        public async Task<IActionResult> PublicarProducto([FromBody] CrearProductoDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos");

            try
            {
                var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
                if (usuario == null) return NotFound("Usuario no encontrado");

                var remate = await _context.Remates.FindAsync(dto.IdRemate);
                if (remate == null) return NotFound("Remate no encontrado");
                if (remate.Estado != "activo") return BadRequest("El remate no está activo");

                var producto = new Producto
                {
                    Titulo = dto.Titulo,
                    Descripcion = dto.Descripcion,
                    PrecioBase = dto.PrecioBase,
                    Imagenes = dto.Imagenes,
                    IdRemate = dto.IdRemate,
                    IdUsuario = dto.IdUsuario,
                    Estado = "Pendiente"
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Producto publicado y pendiente de aprobación", idProducto = producto.IdProducto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }

        
        [HttpGet("pendientes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerProductosPendientes()
        {
            try
            {
                var productos = await _context.Productos.Where(p => p.Estado == "Pendiente").ToListAsync();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }

       
        [HttpPut("aprobar/{idProducto}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AprobarProducto(int idProducto)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(idProducto);
                if (producto == null) return NotFound("Producto no encontrado");
                if (producto.Estado != "Pendiente") return BadRequest("El producto ya fue evaluado");

                producto.Estado = "Aprobado";

                var remate = await _context.Remates.FirstOrDefaultAsync(r => r.Estado == "activo");
                if (remate == null) return BadRequest("No hay remates activos para asociar el producto");

                producto.IdRemate = remate.IdRemate;

                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Producto aprobado y asociado a un remate", idProducto = producto.IdProducto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }
    }
}
