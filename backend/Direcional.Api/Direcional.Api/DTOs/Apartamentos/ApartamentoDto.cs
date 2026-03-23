namespace Direcional.Api.DTOs.Apartamentos
{
    public class ApartamentoDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public int Andar { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
