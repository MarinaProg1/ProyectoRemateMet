using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Factura
{
    public int IdFactura { get; set; }

    public int? IdOferta { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal? Monto { get; set; }

    public virtual Oferta? IdOfertaNavigation { get; set; }
}
