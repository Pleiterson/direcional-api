using System.ComponentModel.DataAnnotations;

namespace Direcional.Api.DTOs.Apartamentos
{
    public class UpdateApartamentoDto
    {
        [Required]
        [MaxLength(10)]
        public string Numero { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "O andar deve ser maior que zero.")]
        public int Andar { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
