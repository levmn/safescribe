using SafeScribe.API.Models;

namespace SafeScribe.API.Interfaces
{
    public interface INoteService
    {
        Task<Note> CreateNoteAsync(string title, string content, int userId);

        Task<Note?> GetNoteByIdAsync(int id);

        Task UpdateNoteAsync(int id, string newTitle, string newContent);

        Task DeleteNoteAsync(int id);
    }
}