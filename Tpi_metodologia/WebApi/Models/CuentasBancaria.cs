using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class CuentasBancaria
{
    public int IdCuenta { get; set; }

    public int? IdUsuario { get; set; }

    public string? NumeroCuenta { get; set; }

    public string? NombreBanco { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
