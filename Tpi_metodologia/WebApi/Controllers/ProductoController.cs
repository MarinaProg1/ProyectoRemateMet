
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Models.DTOs.Producto;
using WebApi.Models.DTOs.Remate;

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
        public async Task<IActionResult> PublicarProducto([FromForm] CrearProductoDto dto) // Usar [FromForm] para aceptar archivos
        {
            if (dto == null) return BadRequest("Datos inválidos");

            try
            {
                // Verificar si el archivo de imagen es válido
                if (dto.Imagen == null || dto.Imagen.Length == 0)
                {
                    return BadRequest("Debe proporcionar una imagen para el producto.");
                }

                // Generar un nombre único para la imagen
                var fileName = Path.GetFileNameWithoutExtension(dto.Imagen.FileName);
                var fileExtension = Path.GetExtension(dto.Imagen.FileName);
                var newFileName = $"{Guid.NewGuid()}{fileExtension}";

                // Ruta de almacenamiento (puedes ajustar esto según tus necesidades)
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Guardar la imagen en el servidor
                var filePath = Path.Combine(uploadPath, newFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Imagen.CopyToAsync(stream);
                }

                // Crear el producto
                var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
                if (usuario == null) return NotFound("Usuario no encontrado");

                var remate = await _context.Remates.FindAsync(dto.IdRemate);
                if (remate == null) return NotFound("Remate no encontrado");
                if (remate.Estado != "abierto") return BadRequest("El remate no está activo");

                var producto = new Producto
                {
                    Titulo = dto.Titulo,
                    Descripcion = dto.Descripcion,
                    PrecioBase = dto.PrecioBase,
                    Imagenes = newFileName, // Guardar solo el nombre del archivo
                    IdRemate = dto.IdRemate,
                    IdUsuario = dto.IdUsuario,
                    Estado = "pendiente"
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
        [Authorize(Roles = "admin")]
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
        [HttpGet("por-remate/{idRemate}")]
        public async Task<IActionResult> ObtenerProductosPorRemate(int idRemate)
        {
            try
            {
                var remate = await _context.Remates.FindAsync(idRemate);
                if (remate == null) return NotFound("Remate no encontrado");

                var productos = await _context.Productos
                    .Where(p => p.IdRemate == idRemate)
                    .ToListAsync();

                if (!productos.Any()) return NotFound("No hay productos en este remate");

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }

     

        [HttpPut("aprobar/{idProducto}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AprobarProducto(int idProducto)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(idProducto);
                if (producto == null) return NotFound("Producto no encontrado");
                if (producto.Estado != "pendiente") return BadRequest("El producto ya fue evaluado");

                producto.Estado = "aprobado";

                var remate = await _context.Remates.FirstOrDefaultAsync(r => r.Estado == "abierto");
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
