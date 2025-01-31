namespace WebApi.Models.DTOs.Remate
{
    public class CrearRemateDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int IdUsuario { get; set; } // Solo el ID, sin la relación completa
    }
}
