using Direcional.Api.Data;
using Direcional.Api.DTOs.Clientes;
using Direcional.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Api.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clientes = await _context.Clientes
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Email = c.Email,
                    Telefone = c.Telefone
                })
                .ToListAsync();

            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == id)
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Email = c.Email,
                    Telefone = c.Telefone
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound();

            return Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
        {
            var exists = await _context.Clientes.AnyAsync(c => c.Email == dto.Email);

            //NOTE: o email é único, verifica se existe um registro com o mesmo email
            if (exists)
                return BadRequest(new { message = "Email já cadastrado." });

            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Email = dto.Email,
                Telefone = dto.Telefone,
                CriadoEm = DateTime.UtcNow
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, new
            {
                cliente.Id,
                cliente.Nome,
                cliente.Email
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClienteDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            cliente.Nome = dto.Nome;
            cliente.Email = dto.Email;
            cliente.Telefone = dto.Telefone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
