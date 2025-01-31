using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApi.Models;
using WebApi;
using WebApi.Models;

namespace WebApi.Custom
{
    public class Utilidades
    {
        private readonly IConfiguration _configuration;
        public Utilidades(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string encriptarSHA256(string texto)
        {
            using (SHA256 sha256Has = SHA256.Create())
            {
                byte[] bytes = sha256Has.ComputeHash(Encoding.UTF8.GetBytes(texto));

                StringBuilder builder1 = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder1.Append(bytes[i].ToString("X2"));

                }
                return builder1.ToString();
            }

        }

        public string generarJWT(Usuario Modelo)
        {
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Modelo.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, Modelo.Email),
                new Claim(ClaimTypes.Role, Modelo.Rol),
            };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            //crear detalle del token
            var jwtConfig = new JwtSecurityToken(
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
        }
    }
}
