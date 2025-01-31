using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string? Email { get; set; }

    public string? Clave { get; set; }

    public string? Direccion { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public string? Rol { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<CuentasBancaria> CuentasBancaria { get; set; } = new List<CuentasBancaria>();

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    public virtual ICollection<Remate> Remates { get; set; } = new List<Remate>();
}
