using Microsoft.AspNetCore.Mvc;
using SafeScribe.API.Interfaces;

namespace SafeScribe.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ITokenBlacklistService _blacklistService;

        public AuthController(IUserService userService, ITokenService tokenService, ITokenBlacklistService blacklistService)
        {
            _userService = userService;
            _tokenService = tokenService;
            _blacklistService = blacklistService;
        }

        [HttpPost("registrar")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registrar(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new { Message = "Nome de usuário e senha são obrigatórios." });
            }

            var user = await _userService.RegisterAsync(username, password, role);

            if (user == null)
            {
                return BadRequest(new { Message = "Usuário já existe ou falha no registro." });
            }

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> Login(string username, string password)
        {
            var user = await _userService.AuthenticateAsync(username, password);

            if (user == null)
            {
                return Unauthorized("Nome de usuário ou senha inválidos.");
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new { Token = token, Username = user.Username, Role = user.Role });
        }

        [HttpPost("logout")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Logout()
        {
            var jti = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                return BadRequest(new { Message = "Token não possui o identificador (JTI) necessário para logout." });
            }

            _blacklistService.AddToBlacklist(jti);

            return Ok(new { Message = "Logout realizado com sucesso. O token foi invalidado." });
        }
    }
}