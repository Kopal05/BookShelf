using Microsoft.EntityFrameworkCore;

namespace BookstaGram.Services
{
    public interface IBookService
    {
        Task AddBook(Book book);
        Task DeleteBook(Book book);
        Task<Book?> GetBookById(int id);
        Task<IEnumerable<Book>> GetBooks();
        Task UpdateBook(Book book);
    }

    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBook(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBook(Book book)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        public async Task<Book?> GetBookById(int id) => await _context.Books.Include(x => x.Genre).Include(x => x.Stock).FirstOrDefaultAsync(x => x.Id == id);

        public async Task<IEnumerable<Book>> GetBooks() => await _context.Books.Include(a=>a.Genre).ToListAsync();
    }
}
