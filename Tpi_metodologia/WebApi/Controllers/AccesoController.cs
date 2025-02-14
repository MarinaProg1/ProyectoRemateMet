
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Login([FromBody] LoginDTO objeto)
        {
            // Verificar que el usuario existe y la contraseña es correcta
            var usuarioEncontrado = await _dbPruebaContext.Usuarios
                .Where(u => u.Email == objeto.Email && u.Clave == _utilidades.encriptarSHA256(objeto.Clave))
                .FirstOrDefaultAsync();

            if (usuarioEncontrado == null)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { isSuccess = false, message = "Credenciales incorrectas" });
            }

            // ✅ Generar el token JWT
            var token = _utilidades.generarJWT(usuarioEncontrado);
          
            // ✅ Configurar la cookie HTTP-Only para almacenar el token
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };

            // Enviar la cookie al cliente
            Response.Cookies.Append("jwt_token", token, cookieOptions);

            // Retornar el token en la respuesta para que MVC pueda usarlo
            return Ok(new { isSuccess = true, token, message = "Inicio de sesión exitoso" });
        }


        [HttpGet("ValidarToken")]
        
        public IActionResult ValidarToken()
        {
            return Ok(new { isValid = true, mensaje = "Token válido" });
        }

    }
}
