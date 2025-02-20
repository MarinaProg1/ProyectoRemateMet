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
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly SubastaMetodologiaDbContext _context;
        
        public ProductoController(SubastaMetodologiaDbContext context)
        {
            _context = context;
        }


        
        [HttpPost("publicar")]
        [Authorize]
        public async Task<IActionResult> PublicarProducto([FromForm] CrearProductoDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos");

            try
            {
                if (dto.Imagen == null || dto.Imagen.Length == 0)
                {
                    return BadRequest("Debe proporcionar una imagen para el producto.");
                }

                var fileExtension = Path.GetExtension(dto.Imagen.FileName);
                var newFileName = $"{Guid.NewGuid()}{fileExtension}";

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes");
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

                // Generar URL pública de la imagen
                var imageUrl = $"{Request.Scheme}://{Request.Host}/api/Producto/imagen/{newFileName}";

                // Validaciones de usuario y remate
                var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
                if (usuario == null) return NotFound("Usuario no encontrado");

                var remate = await _context.Remates.FindAsync(dto.IdRemate);
                if (remate == null) return NotFound("Remate no encontrado");
                if (remate.Estado != "abierto") return BadRequest("El remate no está activo");

                // Crear el producto
                var producto = new Producto
                {
                    Titulo = dto.Titulo,
                    Descripcion = dto.Descripcion,
                    PrecioBase = dto.PrecioBase,
                    Imagenes = imageUrl, // Ahora guarda la URL completa
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
                var productos = await _context.Productos
                    .Where(p => p.IdRemate == idRemate)
                    .ToListAsync();

                if (!productos.Any())
                {
                    // Responder con un código 200 OK y el mensaje
                    return Ok(new { message = "No hay productos para este remate", idRemate });
                }

                return Ok(productos);
            }
            catch (Exception ex)
            {
                // Responder con código 500 en caso de error interno
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
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

        
        [HttpGet("imagen/{fileName}")]
        [AllowAnonymous]
        public IActionResult ObtenerImagen(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", fileName); // Corregido

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Imagen no encontrada");
            }

            var mimeType = "image/jpeg"; // Puedes mejorarlo detectando el tipo MIME automáticamente
            return PhysicalFile(filePath, mimeType);
        }

    }


}
