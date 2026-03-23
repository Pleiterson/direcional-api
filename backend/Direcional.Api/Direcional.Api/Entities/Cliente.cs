using System.ComponentModel.DataAnnotations;

namespace Direcional.Api.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefone { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}
