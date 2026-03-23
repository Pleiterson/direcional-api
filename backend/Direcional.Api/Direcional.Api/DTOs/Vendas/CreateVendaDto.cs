using System.ComponentModel.DataAnnotations;

namespace Direcional.Api.DTOs.Vendas
{
    public class CreateVendaDto
    {
        [Required]
        public Guid ClienteId { get; set; }

        [Required]
        public Guid ApartamentoId { get; set; }

        public Guid? ReservaId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor final deve ser maior que zero.")]
        public decimal ValorFinal { get; set; }
    }
}
