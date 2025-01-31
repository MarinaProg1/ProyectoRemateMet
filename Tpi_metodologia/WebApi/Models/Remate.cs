using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Remate
{
    public int IdRemate { get; set; }

    public string? Titulo { get; set; }

    public string? Descripcion { get; set; }

    public string? Categoria { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaCierre { get; set; }

    public string? Estado { get; set; }

    public decimal? Ganancia { get; set; }

    public int? IdUsuario { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
