
using minimal_api.Dominio.Enums;

namespace minimal_api.Dominio.DTOs
{
    public class AdministradorDTO
    {
        public string Email { get; set; }
        public string Senha { get; set; }
        public Perfil? Perfil { get; set; }
    }
}