
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using System;

using System.IO;

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

                if (dto.Imagenes == null || dto.Imagenes.Length == 0)

                {

                    return BadRequest("Debe proporcionar una imagen para el producto.");

                }



                // Obtener extensión del archivo

                var fileExtension = Path.GetExtension(dto.Imagenes.FileName);



                // Validar extensión de archivo

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(fileExtension.ToLower()))

                {

                    return BadRequest("Formato de imagen no permitido.");

                }



                // Generar nombre único para el archivo

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

                    await dto.Imagenes.CopyToAsync(stream);

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

                    Imagenes = imageUrl,

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

                var productos = await _context.Productos

                  .Where(p => p.Estado == "pendiente")

                  .Select(p => new

                  {

                      p.IdProducto,

                      p.IdRemate,

                      p.Titulo,

                      p.PrecioBase,

                      p.Descripcion,

                      p.Imagenes,

                      p.Estado

                  })

                  .ToListAsync();



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

                  .Select(p => new

                  {

                      p.IdProducto,

                      p.Titulo,

                      p.PrecioBase,

                      p.Descripcion,

                      p.Imagenes,

                      p.Estado

                  })

                  .ToListAsync();



                if (!productos.Any())

                {

                    return Ok(new { message = "No hay productos para este remate", idRemate });

                }



                return Ok(productos);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });

            }

        }





        [HttpPut("aprobar/{idProducto}/{idRemate}")]

        [Authorize(Roles = "admin")]

        public async Task<IActionResult> AprobarProducto(int idProducto, int idRemate)

        {

            try

            {

                // Buscar el producto por su ID

                var producto = await _context.Productos.FindAsync(idProducto);

                if (producto == null)

                    return NotFound("Producto no encontrado");



                // Verificar si el producto ya fue evaluado

                if (producto.Estado != "pendiente")

                    return BadRequest("El producto ya fue evaluado");



                // Aprobar el producto

                producto.Estado = "aprobado";



                // Buscar el remate con el ID especificado y que esté abierto

                var remate = await _context.Remates.FirstOrDefaultAsync(r => r.IdRemate == idRemate && r.Estado == "abierto");

                if (remate == null)

                    return BadRequest("El remate especificado no está activo o no existe.");



                // Asociar el producto al remate correcto

                producto.IdRemate = remate.IdRemate;



                // Actualizar el producto en la base de datos

                _context.Productos.Update(producto);

                await _context.SaveChangesAsync();



                return Ok(new { mensaje = "Producto aprobado y asociado al remate especificado", idProducto = producto.IdProducto });

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

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", fileName);



            if (!System.IO.File.Exists(filePath))

            {

                return NotFound("Imagen no encontrada");

            }



            // Detectar el tipo MIME automáticamente

            var mimeType = $"imagen/{Path.GetExtension(fileName).Trim('.').ToLower()}";



            return PhysicalFile(filePath, mimeType);

        }



    }

}