using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeScribe.API.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SafeScribe.API.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        // POST 'api/v1/notas'
        // Apenas 'Editor' e 'Admin' podem criar notas.
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CriarNota(string title, string content)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var newNote = await _noteService.CreateNoteAsync(title, content, userId.Value);

            return CreatedAtAction(nameof(ObterNota), new { id = newNote.Id }, newNote);
        }

        // GET 'api/v1/notas/{id}'
        // Todos os perfis logados podem ler, mas com restrições de posse para 'Leitor' e 'Editor'.
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObterNota(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var note = await _noteService.GetNoteByIdAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            var userRole = GetCurrentUserRole();

            if (userRole != "Admin" && note.UserId != userId)
            {
                return Forbid();
            }

            return Ok(note);
        }

        // PUT 'api/v1/notas/{id}'
        // Apenas 'Editor' e 'Admin' podem atualizar.
        [HttpPut("{id}")]
        [Authorize(Roles = "Editor,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarNota(int id, string newTitle, string newContent)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var note = await _noteService.GetNoteByIdAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            var userRole = GetCurrentUserRole();

            if (userRole == "Editor" && note.UserId != userId)
            {
                return Forbid();
            }

            await _noteService.UpdateNoteAsync(id, newTitle, newContent);

            return NoContent();
        }

        // DELETE 'api/v1/notas/{id}'
        // Apenas 'Admin' pode apagar notas.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ApagarNota(int id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            await _noteService.DeleteNoteAsync(id);
            return NoContent();
        }
    }
}