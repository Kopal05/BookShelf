using Microsoft.EntityFrameworkCore;

namespace BookstaGram.Services;

public interface IGenreService
{
    Task AddGenre(Genre genre);
    Task UpdateGenre(Genre genre);
    Task<Genre?> GetGenreById(int id);
    Task DeleteGenre(Genre genre);
    Task<IEnumerable<Genre>> GetGenres();
}

public class GenreService : IGenreService
{
    private readonly ApplicationDbContext _context;
    public GenreService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddGenre(Genre genre)
    {
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateGenre(Genre genre)
    {
        _context.Genres.Update(genre);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteGenre(Genre genre)
    {
        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
    }

    public async Task<Genre?> GetGenreById(int id)
    {
        return await _context.Genres.FindAsync(id);
    }

    public async Task<IEnumerable<Genre>> GetGenres()
    {
        return await _context.Genres.ToListAsync();
    }    
}
