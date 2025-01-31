using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Oferta
{
    public int IdOferta { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdProducto { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal? Monto { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual Producto? IdProductoNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
