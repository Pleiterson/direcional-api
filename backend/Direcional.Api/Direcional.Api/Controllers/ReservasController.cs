using Direcional.Api.Data;
using Direcional.Api.DTOs.Reservas;
using Direcional.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Direcional.Api.Controllers
{
    [ApiController]
    [Route("api/reservas")]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Apartamento)
                .Include(r => r.Corretor)
                .Select(r => new ReservaDto
                {
                    Id = r.Id,
                    ClienteId = r.ClienteId,
                    ClienteNome = r.Cliente.Nome,
                    ApartamentoId = r.ApartamentoId,
                    ApartamentoNumero = r.Apartamento.Numero,
                    CorretorId = r.CorretorId,
                    CorretorNome = r.Corretor != null ? r.Corretor.Nome : null,
                    Status = r.Status,
                    DataReserva = r.DataReserva
                })
                .ToListAsync();

            return Ok(reservas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Apartamento)
                .Include(r => r.Corretor)
                .Where(r => r.Id == id)
                .Select(r => new ReservaDto
                {
                    Id = r.Id,
                    ClienteId = r.ClienteId,
                    ClienteNome = r.Cliente.Nome,
                    ApartamentoId = r.ApartamentoId,
                    ApartamentoNumero = r.Apartamento.Numero,
                    CorretorId = r.CorretorId,
                    CorretorNome = r.Corretor != null ? r.Corretor.Nome : null,
                    Status = r.Status,
                    DataReserva = r.DataReserva
                })
                .FirstOrDefaultAsync();

            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada." });

            return Ok(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservaDto dto)
        {
            //IMPORTANT: Regra de Negócio: toda reserva deve estar associada a um cliente existente
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return BadRequest(new { message = "Cliente não encontrado." });

            //IMPORTANT: Regra de Negócio: toda reserva deve estar associada a um apartamento existente
            var apartamento = await _context.Apartamentos.FindAsync(dto.ApartamentoId);
            if (apartamento == null)
                return BadRequest(new { message = "Apartamento não encontrado." });

            //IMPORTANT: Regra de Negócio: só é permitido reservar apartamentos com status "Disponível"
            //se estiver "Reservado" ou "Vendido", a operação deve ser bloqueada
            if (apartamento.Status != "Disponível")
            {
                return BadRequest(new
                {
                    message = "O apartamento não está disponível para reserva."
                });
            }

            //IMPORTANT: Regra de Negócio: a reserva deve registrar qual corretor autenticado realizou a operação
            var corretorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(corretorIdClaim))
                return Unauthorized(new { message = "Corretor autenticado não identificado." });

            if (!Guid.TryParse(corretorIdClaim, out var corretorId))
                return Unauthorized(new { message = "Identificador do corretor inválido." });

            var corretor = await _context.Corretores.FindAsync(corretorId);
            if (corretor == null || !corretor.Ativo)
                return Unauthorized(new { message = "Corretor não encontrado ou inativo." });

            //IMPORTANT: Regra de Negócio: ao criar a reserva, ela nasce com status "Ativo"
            var reserva = new Reserva
            {
                Id = Guid.NewGuid(),
                ClienteId = dto.ClienteId,
                ApartamentoId = dto.ApartamentoId,
                CorretorId = corretorId,
                Status = "Ativa",
                DataReserva = DateTime.UtcNow
            };

            //IMPORTANT: Regra de Negócio: ao reservar um apartamento, o status do apartamento deve mudar para "Reservado"
            apartamento.Status = "Reservado";

            _context.Reservas.Add(reserva);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, new
            {
                reserva.Id,
                reserva.ClienteId,
                reserva.ApartamentoId,
                reserva.CorretorId,
                reserva.Status,
                reserva.DataReserva
            });
        }

        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Apartamento)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
                return NotFound(new { message = "Reserva não encontrada." });

            //IMPORTANT: Regra de Negócio: apenas reservas ativas podem ser canceladas
            if (reserva.Status != "Ativa")
            {
                return BadRequest(new
                {
                    message = "Somente reservas ativas podem ser canceladas."
                });
            }

            //IMPORTANT: Regra de Negócio: ao cancelar uma reserva, o status da reserva deve mudar para "Cancelada"
            //e o apartamento volta a ficar "Disponível"
            reserva.Status = "Cancelada";
            reserva.Apartamento.Status = "Disponível";

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
