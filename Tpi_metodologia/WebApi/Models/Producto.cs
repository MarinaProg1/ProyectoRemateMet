using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string? Titulo { get; set; }

    public string? Descripcion { get; set; }

    public decimal? PrecioBase { get; set; }

    public string? Imagenes { get; set; }

    public string? Estado { get; set; }

    public int? IdRemate { get; set; }

    public int? IdUsuario { get; set; }

    public virtual Remate? IdRemateNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();
}
