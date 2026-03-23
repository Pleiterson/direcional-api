
using System.ComponentModel.DataAnnotations;

namespace Direcional.Api.DTOs.Vendas
{
    public class VendaDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public Guid ApartamentoId { get; set; }
        public string ApartamentoNumero { get; set; } = string.Empty;
        public Guid? ReservaId { get; set; }
        public Guid? CorretorId { get; set; }
        public string? CorretorNome { get; set; }
        public decimal ValorFinal { get; set; }
        public DateTime DataVenda { get; set; }
    }
}
