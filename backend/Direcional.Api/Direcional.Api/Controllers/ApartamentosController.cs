using Direcional.Api.Data;
using Direcional.Api.DTOs.Apartamentos;
using Direcional.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Api.Controllers
{
    [ApiController]
    [Route("api/apartamentos")]
    [Authorize]
    public class ApartamentosController : ControllerBase
    {
        private static readonly string[] StatusPermitidos =
    [
        "Disponível",
        "Reservado",
        "Vendido"
    ];

        private readonly AppDbContext _context;

        public ApartamentosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var apartamentos = await _context.Apartamentos
                .Select(a => new ApartamentoDto
                {
                    Id = a.Id,
                    Numero = a.Numero,
                    Andar = a.Andar,
                    Valor = a.Valor,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(apartamentos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var apartamento = await _context.Apartamentos
                .Where(a => a.Id == id)
                .Select(a => new ApartamentoDto
                {
                    Id = a.Id,
                    Numero = a.Numero,
                    Andar = a.Andar,
                    Valor = a.Valor,
                    Status = a.Status
                })
                .FirstOrDefaultAsync();

            if (apartamento == null)
                return NotFound(new { message = "Apartamento não encontrado." });

            return Ok(apartamento);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApartamentoDto dto)
        {
            if (!StatusPermitidos.Contains(dto.Status))
            {
                return BadRequest(new
                {
                    message = "Status inválido. Valores permitidos: Disponível, Reservado, Vendido."
                });
            }

            //IMPORTANT: Regra de egócio: não permitir cadastro duplicado do mesmo número/andar
            var exists = await _context.Apartamentos
                .AnyAsync(a => a.Numero == dto.Numero && a.Andar == dto.Andar);

            if (exists)
            {
                return BadRequest(new
                {
                    message = "Já existe um apartamento cadastrado com o mesmo número e andar."
                });
            }

            var apartamento = new Apartamento
            {
                Id = Guid.NewGuid(),
                Numero = dto.Numero,
                Andar = dto.Andar,
                Valor = dto.Valor,
                Status = dto.Status,
                CriadoEm = DateTime.UtcNow
            };

            _context.Apartamentos.Add(apartamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = apartamento.Id }, new
            {
                apartamento.Id,
                apartamento.Numero,
                apartamento.Andar,
                apartamento.Valor,
                apartamento.Status
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApartamentoDto dto)
        {
            if (!StatusPermitidos.Contains(dto.Status))
            {
                return BadRequest(new
                {
                    message = "Status inválido. Valores permitidos: Disponível, Reservado, Vendido."
                });
            }

            var apartamento = await _context.Apartamentos.FindAsync(id);

            if (apartamento == null)
                return NotFound(new { message = "Apartamento não encontrado." });

            //IMPORTANT: Regra de egócio: evitar conflito de número/andar com outro apartamento já existente
            var exists = await _context.Apartamentos
                .AnyAsync(a => a.Id != id && a.Numero == dto.Numero && a.Andar == dto.Andar);

            if (exists)
            {
                return BadRequest(new
                {
                    message = "Já existe outro apartamento cadastrado com o mesmo número e andar."
                });
            }

            apartamento.Numero = dto.Numero;
            apartamento.Andar = dto.Andar;
            apartamento.Valor = dto.Valor;
            apartamento.Status = dto.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var apartamento = await _context.Apartamentos.FindAsync(id);

            if (apartamento == null)
                return NotFound(new { message = "Apartamento não encontrado." });

            //IMPORTANT: Regra de egócio: não permitir exclusão de apartamento já vendido
            if (apartamento.Status == "Vendido")
            {
                return BadRequest(new
                {
                    message = "Não é permitido excluir um apartamento já vendido."
                });
            }

            _context.Apartamentos.Remove(apartamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
