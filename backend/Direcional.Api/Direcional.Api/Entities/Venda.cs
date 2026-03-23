namespace Direcional.Api.Entities
{
    public class Venda
    {
        public Guid Id { get; set; }

        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public Guid ApartamentoId { get; set; }
        public Apartamento Apartamento { get; set; } = null!;

        public Guid? ReservaId { get; set; }
        public Reserva? Reserva { get; set; }

        public Guid? CorretorId { get; set; }
        public Corretor? Corretor { get; set; }

        public decimal ValorFinal { get; set; }

        public DateTime DataVenda { get; set; } = DateTime.UtcNow;
    }
}
