using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models.DTOs.Remate;
using System.Text.Json.Serialization;
using System.Text.Json;
namespace WebApi.Controllers;

[Route("api/remates")]
[ApiController]
public class RematesController : ControllerBase
{
    private readonly SubastaMetodologiaDbContext _context;
    private readonly RemateService _remateService;    
    public RematesController(SubastaMetodologiaDbContext context, RemateService remateService)
    {
        _context = context;
        _remateService = remateService;
    }

    [HttpPost("crear")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CrearRemate([FromBody] CrearRemateDto dto)
    {
        if (dto == null) return BadRequest("Datos inválidos");
        if (dto.IdUsuario <= 0) return BadRequest("ID de usuario inválido");

        try
        {
            var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
            if (usuario == null) return NotFound("Usuario no encontrado");

            var remate = new Remate
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Categoria = dto.Categoria,
                FechaInicio = DateTime.Now.AddDays(4),
                FechaCierre = DateTime.Now.AddDays(11),
                Estado = "preparacion",
                IdUsuario = dto.IdUsuario
            };

            _context.Remates.Add(remate);
            await _context.SaveChangesAsync();

            // No se calcula la ganancia aquí porque aún no hay productos

            return Ok(new { mensaje = "Remate creado en estado 'Preparación'", idRemate = remate.IdRemate });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpGet("activas")]
    public async Task<IActionResult> ObtenerSubastasActivas()
    {
        try
        {
            var remates = await _context.Remates
                .Where(r => r.Estado == "abierto")
                .Include(r => r.Productos)
                .ToListAsync();

            if (remates.Count == 0) return NotFound("No hay subastas activas");

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };

            return Ok(JsonSerializer.Serialize(remates, options));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpPost("actualizar-estados")]
    public async Task<IActionResult> ActualizarEstados()
    {
        try
        {
            var ahora = DateTime.Now;
            var remates = await _context.Remates.ToListAsync();

            if (remates.Count == 0) return NotFound("No hay remates registrados");

            foreach (var remate in remates)
            {
                // De 'preparación' a 'abierto'
                if (remate.Estado == "preparación" && remate.FechaInicio <= ahora && remate.FechaCierre > ahora)
                {
                    remate.Estado = "abierto";
                }
                // De 'abierto' a 'cerrado'
                else if (remate.Estado == "abierto" && remate.FechaCierre <= ahora)
                {
                    remate.Estado = "cerrado";
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Estados actualizados");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }


    [HttpGet("todos")]
    public async Task<IActionResult> ObtenerTodosLosRemates()
    {
        try
        {

            var remates = await _context.Remates
                .Include(r => r.Productos)
                .ToListAsync();

            if (remates.Count == 0)
                return NotFound("No hay remates registrados");

            return Ok(remates);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpPost("procesar-remates")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ProcesarRemates()
    {
        var servicio = new RemateService(_context);
        await servicio.ProcesarRematesCerrados();
        return Ok("Remates cerrados procesados correctamente.");
    }

    [HttpGet("{idRemate}")]
    public async Task<IActionResult> ObtenerRematePorId(int idRemate)
    {
        try
        {
            var remate = await _context.Remates
                .FirstOrDefaultAsync(r => r.IdRemate == idRemate);

            if (remate == null)
            {
                return NotFound($"No se encontró un remate con el ID {idRemate}");
            }

            return Ok(remate);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

   

    [HttpPost("calcular-oferta-ganadora/{idProducto}")]
   
    public async Task<IActionResult> CalcularOfertaGanadoraPorProducto(int idProducto)
    {
        try
        {
            var resultado = await _remateService.CalcularOfertaGanadoraPorProducto(idProducto);

            if (resultado == null)
            {
                return NotFound("No hay oferta ganadora para este producto.");
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpPost("calcular-ofertas-ganadoras")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CalcularOfertasGanadoras()
    {
        try
        {
            var resultados = await _remateService.CalcularOfertasGanadoras();

            if (resultados.Count == 0)
            {
                return NotFound("No hubo ofertas ganadoras.");
            }

            return Ok(resultados);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }


}
