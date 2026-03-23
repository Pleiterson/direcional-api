using System.ComponentModel.DataAnnotations;

namespace Direcional.Api.Entities
{
    public class Reserva
    {
        public Guid Id { get; set; }

        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public Guid ApartamentoId { get; set; }
        public Apartamento Apartamento { get; set; } = null!;

        public Guid? CorretorId { get; set; }
        public Corretor? Corretor { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        public DateTime DataReserva { get; set; } = DateTime.UtcNow;
    }
}
