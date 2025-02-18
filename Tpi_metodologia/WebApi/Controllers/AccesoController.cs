
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Custom;
using WebApi.Models.DTOs.Usuario;
using WebApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly SubastaMetodologiaDbContext _dbPruebaContext;
        private readonly Utilidades _utilidades;

        public AccesoController(SubastaMetodologiaDbContext dbPruebaContext, Utilidades utilidades)
        {
            _dbPruebaContext = dbPruebaContext;
            _utilidades = utilidades;
        }

        [HttpPost]
        [Route("Registrarse")]
        [AllowAnonymous]
        public async Task<IActionResult> Registrarse([FromBody] UsuarioDTO objeto)
        {
            if (objeto == null)
                return BadRequest("El objeto recibido es nulo.");

            try
            {
                var modeloUsuario = new Usuario
                {
                    Nombre = objeto.Nombre,
                    Apellido = objeto.Apellido,
                    Email = objeto.Email,
                    Clave = _utilidades.encriptarSHA256(objeto.Clave),
                    Direccion = objeto.Direccion,
                    FechaNacimiento = objeto.FechaNacimiento,
                    Rol = objeto.Rol,
                    Estado = objeto.Estado,
                };

                await _dbPruebaContext.Usuarios.AddAsync(modeloUsuario);
                await _dbPruebaContext.SaveChangesAsync();

                return Ok(new { isSuccess = modeloUsuario.IdUsuario != 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO objeto)
        {
            var usuarioEncontrado = await _dbPruebaContext.Usuarios
                .Where(u => u.Email == objeto.Email && u.Clave == _utilidades.encriptarSHA256(objeto.Clave))
                .FirstOrDefaultAsync();

            if (usuarioEncontrado == null)
            {
                return Unauthorized(new { isSuccess = false, message = "Credenciales incorrectas" });
            }

            var token = _utilidades.generarJWT(usuarioEncontrado);

            return Ok(new {token});
        }

        [HttpGet("ValidarToken")]
        [Authorize]
        public IActionResult ValidarToken()
        {
            return Ok(new { isValid = true, mensaje = "Token válido" });
        }
    }
}
