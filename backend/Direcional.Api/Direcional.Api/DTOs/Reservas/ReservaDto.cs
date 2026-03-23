namespace Direcional.Api.DTOs.Reservas
{
    public class ReservaDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public Guid ApartamentoId { get; set; }
        public string ApartamentoNumero { get; set; } = string.Empty;
        public Guid? CorretorId { get; set; }
        public string? CorretorNome { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DataReserva { get; set; }
    }
}
