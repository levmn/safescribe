using SafeScribe.API.Interfaces;
using SafeScribe.API.Models;
using SafeScribe.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SafeScribe.API.Services
{
    public class NoteService : INoteService
    {
        private readonly AppDbContext _context;

        public NoteService(AppDbContext context)
        {
            _context = context;
            SeedData();
        }

        private void SeedData()
        {
            if (!_context.Notes.Any() && _context.Users.Any())
            {
                var adminId = _context.Users.FirstOrDefault(u => u.Username == "admin")?.Id ?? 1;
                var editorId = _context.Users.FirstOrDefault(u => u.Username == "editor")?.Id ?? 2;

                _context.Notes.AddRange(
                    new Note { Title = "Relatório Administrativo", Content = "Conteúdo sobre finanças.", CreatedAt = DateTime.UtcNow, UserId = adminId },
                    new Note { Title = "Lembrete de Código", Content = "Verificar middleware de blacklist.", CreatedAt = DateTime.UtcNow.AddHours(-1), UserId = editorId }
                );

                _context.SaveChanges();
            }
        }

        public async Task<Note> CreateNoteAsync(string title, string content, int userId)
        {
            var newNote = new Note
            {
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _context.Notes.AddAsync(newNote);
            await _context.SaveChangesAsync();

            return newNote;
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            return await _context.Notes
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task UpdateNoteAsync(int id, string newTitle, string newContent)
        {
            var noteToUpdate = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);

            if (noteToUpdate != null)
            {
                noteToUpdate.Title = newTitle;
                noteToUpdate.Content = newContent;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNoteAsync(int id)
        {
            var noteToRemove = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);

            if (noteToRemove != null)
            {
                _context.Notes.Remove(noteToRemove);
                await _context.SaveChangesAsync();
            }
        }
    }
}