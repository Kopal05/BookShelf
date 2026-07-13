
using Microsoft.EntityFrameworkCore;

namespace BookstaGram.Services
{
    public interface IHomeService
    {
        Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0);
        Task<IEnumerable<Genre>> Genres();
    }

    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _db;

        public HomeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Genre>> Genres()
        {
            return await _db.Genres.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0)
        {
            
            // In this code we are first building query, then rebuilding that query on the basis of filter. Query is translated into sql when we call .ToListAsync() method.

            // LINQ being used here --- 

            var bookQuery = _db.Books
               .AsNoTracking()
               .Include(x => x.Genre)
               .Include(x => x.Stock)
               .AsQueryable();

            if (!string.IsNullOrWhiteSpace(sTerm))
            {
                bookQuery = bookQuery.Where(b => b.BookName.StartsWith(sTerm.ToLower()));
            }

            if (genreId > 0)
            {
                bookQuery = bookQuery.Where(b => b.GenreId == genreId);
            }

            var books = await bookQuery
                .AsNoTracking()
                .Select(book => new Book
                {
                    Id = book.Id,
                    Image = book.Image,
                    AuthorName = book.AuthorName,
                    BookName = book.BookName,
                    GenreId = book.GenreId,
                    Price = book.Price,
                    GenreName = book.Genre.GenreName,
                    Quantity = book.Stock == null ? 0 : book.Stock.Quantity
                }).ToListAsync();
            
            return books;

        }
    }
}
