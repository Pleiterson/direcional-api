using System.ComponentModel.DataAnnotations;

namespace Direcional.Api.DTOs.Clientes
{
    public class CreateClienteDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Telefone { get; set; }
    }
}
