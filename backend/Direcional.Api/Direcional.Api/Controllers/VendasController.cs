using Direcional.Api.Data;
using Direcional.Api.DTOs.Vendas;
using Direcional.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Direcional.Api.Controllers
{
    [ApiController]
    [Route("api/vendas")]
    [Authorize]
    public class VendasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VendasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vendas = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Apartamento)
                .Include(v => v.Corretor)
                .Select(v => new VendaDto
                {
                    Id = v.Id,
                    ClienteId = v.ClienteId,
                    ClienteNome = v.Cliente.Nome,
                    ApartamentoId = v.ApartamentoId,
                    ApartamentoNumero = v.Apartamento.Numero,
                    ReservaId = v.ReservaId,
                    CorretorId = v.CorretorId,
                    CorretorNome = v.Corretor != null ? v.Corretor.Nome : null,
                    ValorFinal = v.ValorFinal,
                    DataVenda = v.DataVenda
                })
                .ToListAsync();

            return Ok(vendas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Apartamento)
                .Include(v => v.Corretor)
                .Where(v => v.Id == id)
                .Select(v => new VendaDto
                {
                    Id = v.Id,
                    ClienteId = v.ClienteId,
                    ClienteNome = v.Cliente.Nome,
                    ApartamentoId = v.ApartamentoId,
                    ApartamentoNumero = v.Apartamento.Numero,
                    ReservaId = v.ReservaId,
                    CorretorId = v.CorretorId,
                    CorretorNome = v.Corretor != null ? v.Corretor.Nome : null,
                    ValorFinal = v.ValorFinal,
                    DataVenda = v.DataVenda
                })
                .FirstOrDefaultAsync();

            if (venda == null)
                return NotFound(new { message = "Venda não encontrada." });

            return Ok(venda);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVendaDto dto)
        {
            //IMPORTANT: Regra de Negócio: toda venda deve estar associada a um cliente existente
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return BadRequest(new { message = "Cliente não encontrado." });

            //IMPORTANT: Regra de Negócio: toda venda deve estar associada a um apartamento existente
            var apartamento = await _context.Apartamentos.FindAsync(dto.ApartamentoId);
            if (apartamento == null)
                return BadRequest(new { message = "Apartamento não encontrado." });

            //IMPORTANT: Regra de Negócio: a venda só pode ser concluída se o apartamento estiver reservado
            //isso evita venda direta de apartamento disponível ou já vendido
            if (apartamento.Status != "Reservado")
            {
                return BadRequest(new
                {
                    message = "A venda só pode ser realizada para um apartamento reservado."
                });
            }

            //IMPORTANT: Regra de Negócio: a venda deve registrar qual corretor autenticado realizou a operação
            var corretorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(corretorIdClaim))
                return Unauthorized(new { message = "Corretor autenticado não identificado." });

            if (!Guid.TryParse(corretorIdClaim, out var corretorId))
                return Unauthorized(new { message = "Identificador do corretor inválido." });

            var corretor = await _context.Corretores.FindAsync(corretorId);
            if (corretor == null || !corretor.Ativo)
                return Unauthorized(new { message = "Corretor não encontrado ou inativo." });

            Reserva? reserva = null;

            if (dto.ReservaId.HasValue)
            {
                reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.Id == dto.ReservaId.Value);

                //IMPORTANT: Regra de Negócio: se a venda informar uma reserva, ela precisa existir
                if (reserva == null)
                    return BadRequest(new { message = "Reserva não encontrada." });

                //IMPORTANT: Regra de Negócio: a reserva precisa estar ativa para ser convertida em venda
                if (reserva.Status != "Ativa")
                {
                    return BadRequest(new
                    {
                        message = "A reserva informada não está ativa."
                    });
                }

                //IMPORTANT: Regra de Negócio: a reserva precisa pertencer ao mesmo cliente e apartamento da venda
                if (reserva.ClienteId != dto.ClienteId || reserva.ApartamentoId != dto.ApartamentoId)
                {
                    return BadRequest(new
                    {
                        message = "A reserva não corresponde ao cliente e apartamento informados."
                    });
                }
            }
            else
            {
                //IMPORTANT: Regra de Negócio: se a reserva não for enviada explicitamente, tentamos encontrar
                //uma reserva ativa para o mesmo cliente e apartamento
                reserva = await _context.Reservas.FirstOrDefaultAsync(r =>
                    r.ClienteId == dto.ClienteId &&
                    r.ApartamentoId == dto.ApartamentoId &&
                    r.Status == "Ativa");

                if (reserva == null)
                {
                    return BadRequest(new
                    {
                        message = "Não existe reserva ativa para este cliente e apartamento."
                    });
                }
            }

            //IMPORTANT: Regra de Negócio: um apartamento não pode ter mais de uma venda registrada
            var vendaExistente = await _context.Vendas.AnyAsync(v => v.ApartamentoId == dto.ApartamentoId);
            if (vendaExistente)
            {
                return BadRequest(new
                {
                    message = "Já existe uma venda registrada para este apartamento."
                });
            }

            var venda = new Venda
            {
                Id = Guid.NewGuid(),
                ClienteId = dto.ClienteId,
                ApartamentoId = dto.ApartamentoId,
                ReservaId = reserva.Id,
                CorretorId = corretorId,
                ValorFinal = dto.ValorFinal,
                DataVenda = DateTime.UtcNow
            };

            //IMPORTANT: Regra de Negócio: ao concluir a venda, o apartamento passa para o status "Vendido".
            apartamento.Status = "Vendido";

            //IMPORTANT: Regra de Negócio: o concluir a venda, a reserva ativa é finalizada como "Fechada".
            reserva.Status = "Fechada";

            _context.Vendas.Add(venda);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = venda.Id }, new
            {
                venda.Id,
                venda.ClienteId,
                venda.ApartamentoId,
                venda.ReservaId,
                venda.CorretorId,
                venda.ValorFinal,
                venda.DataVenda
            });
        }
    }
}
