using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

public class RemateService
{
    private readonly SubastaMetodologiaDbContext _context;

    public RemateService(SubastaMetodologiaDbContext context)
    {
        _context = context;
    }

    public async Task ProcesarRematesCerrados()
    {
        var rematesCerrados = await _context.Remates
            .Where(r => r.Estado == "cerrado")
            .ToListAsync();

        foreach (var remate in rematesCerrados)
        {
            var productos = await _context.Productos
                .Where(p => p.IdRemate == remate.IdRemate)
                .ToListAsync();

            foreach (var producto in productos)
            {
                var ofertaGanadora = await _context.Ofertas
                    .Where(o => o.IdProducto == producto.IdProducto)
                    .OrderByDescending(o => o.Monto)
                    .ThenBy(o => o.Fecha)
                    .FirstOrDefaultAsync();

                if (ofertaGanadora != null)
                {
                    ofertaGanadora.Estado = "ganadora";

                    // Crear factura
                    var factura = new Factura
                    {
                        IdOferta = ofertaGanadora.IdOferta,
                        Fecha = DateTime.Now,
                        Monto = ofertaGanadora.Monto
                    };

                    _context.Facturas.Add(factura);
                    await _context.SaveChangesAsync();

                    // Enviar correo al ganador
                    var usuario = await _context.Usuarios.FindAsync(ofertaGanadora.IdUsuario);
                    if (usuario != null)
                    {
                        EnviarCorreoFactura(usuario.Email, usuario.Nombre, factura);
                    }
                }
            }

            // Marcar el remate como procesado
            remate.Estado = "cerrado";
        }

        await _context.SaveChangesAsync();
    }

    public void EnviarCorreoFactura(string email, string nombre, Factura factura)
    {
        try
        {
            var fromAddress = new MailAddress("tuemail@gmail.com", "TUP Remates");
            var toAddress = new MailAddress(email, nombre);
            const string fromPassword = "tu-contraseña";
            const string subject = "Factura de Subasta Ganada";

            string body = $"Hola {nombre},\n\n" +
                          $"Has ganado una subasta. Adjuntamos los detalles de tu factura:\n\n" +
                          $"ID de Factura: {factura.IdFactura}\n" +
                          $"Fecha: {factura.Fecha}\n" +
                          $"Monto: {factura.Monto:C}\n\n" +
                          $"Gracias por participar en TUP Remates.";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            smtp.Send(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar correo: {ex.Message}");
        }
    }
    public async Task<dynamic> CalcularOfertaGanadoraPorProducto(int idProducto)
    {
        // Obtener la oferta más alta para el producto específico
        var ofertaGanadora = await _context.Ofertas
            .Where(o => o.IdProducto == idProducto)
            .OrderByDescending(o => o.Monto)
            .ThenBy(o => o.Fecha)
            .FirstOrDefaultAsync();

        if (ofertaGanadora != null)
        {
            // Marcar oferta como ganadora
            ofertaGanadora.Estado = "ganadora";

            // Crear factura para la oferta ganadora
            var factura = new Factura
            {
                IdOferta = ofertaGanadora.IdOferta,
                Fecha = DateTime.Now,
                Monto = ofertaGanadora.Monto
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            // Obtener información del usuario ganador
            var usuario = await _context.Usuarios
                .Where(u => u.IdUsuario == ofertaGanadora.IdUsuario)
                .FirstOrDefaultAsync();

            if (usuario != null)
            {
                // Notificar al usuario ganador
                EnviarCorreoFactura(usuario.Email, usuario.Nombre, factura);

                // Retornar el resultado con nombre y monto
                return new
                {
                    NombreUsuario = usuario.Nombre,
                    MontoOferta = ofertaGanadora.Monto
                };
            }
        }

        // Retornar null si no hay oferta ganadora
        return null;
    }


}

