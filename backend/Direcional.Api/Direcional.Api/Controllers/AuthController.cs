using Direcional.Api.Data;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Direcional.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var corretor = await _context.Corretores
                .FirstOrDefaultAsync(x => x.Email == request.Email && x.Ativo);

            //IMPORTANT: Regra de negócio: bloqueia o acesso se o corretor não existir ou estiver inativo.
            if (corretor is null)
                return Unauthorized(new { message = "Usuário ou senha inválidos." });

            //NOTE: Regra de segurança: a senha informada é comparada com o hash BCrypt armazenado no banco.
            var senhaValida = BCrypt.Net.BCrypt.Verify(request.Password, corretor.SenhaHash);

            if (!senhaValida)
                return Unauthorized(new { message = "Usuário ou senha inválidos." });

            var key = _configuration["Jwt:Key"]!;
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]!);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, corretor.Id.ToString()),
                    new Claim(ClaimTypes.Name, corretor.Nome),
                    new Claim(ClaimTypes.Email, corretor.Email),
                    new Claim(ClaimTypes.Role, "Corretor")
                ]),
                Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiresInMinutes,
                corretor = new
                {
                    corretor.Id,
                    corretor.Nome,
                    corretor.Email
                }
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
