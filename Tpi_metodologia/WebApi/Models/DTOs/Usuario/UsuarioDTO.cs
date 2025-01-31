namespace WebApi.Models.DTOs.Usuario
{
    public class UsuarioDTO
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Clave { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Rol { get; set; }
        public string Estado { get; set; }
    }
}
