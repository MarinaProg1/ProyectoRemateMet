namespace WebApi.Models.DTOs.Remate
{
    public class RemateDto
    {
        public int IdRemate { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCierre { get; set; }
        public string Estado { get; set; } = string.Empty;
       // public decimal? Ganancia { get; set; }
        public int? IdUsuario { get; set; }
    }
}
