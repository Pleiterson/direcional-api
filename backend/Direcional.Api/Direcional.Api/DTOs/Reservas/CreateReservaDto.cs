using System.ComponentModel.DataAnnotations;

namespace Direcional.Api.DTOs.Reservas
{
    public class CreateReservaDto
    {
        [Required]
        public Guid ClienteId { get; set; }

        [Required]
        public Guid ApartamentoId { get; set; }
    }
}
