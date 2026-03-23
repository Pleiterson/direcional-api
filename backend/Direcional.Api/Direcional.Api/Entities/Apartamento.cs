using System.ComponentModel.DataAnnotations;

namespace Direcional.Api.Entities
{
    public class Apartamento
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Numero { get; set; } = string.Empty;

        public int Andar { get; set; }

        public decimal Valor { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}
