namespace WebApi.Models.DTOs.Producto
{
    public class ProductoDto
    {
        public int IdProducto { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public string imagenes { get; set; }
        public string? Estado { get; set; }
        public int IdRemate { get; set; } // El usuario debe indicar a qué remate pertenece el producto
        public int IdUsuario { get; set; } // ID del usuario que publica el producto
    }
}
