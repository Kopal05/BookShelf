using BookstaGram.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BookstaGram.Controllers;

[Authorize]
public class BookController : Controller
{
    private readonly IBookService _bookService;
    private readonly IGenreService _genreService;
    private readonly IFileService _fileService;

    public BookController(IBookService bookRepo, IGenreService genreRepo, IFileService fileService)
    {
        _bookService = bookRepo;
        _genreService = genreRepo;
        _fileService = fileService;
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetBooks();
        return View(books);
    }
    
    public async Task<IActionResult> ViewBook(int id)
    {
        var book = await _bookService.GetBookById(id);
        if (book == null)
        {
            TempData["errorMessage"] = $"Book with the id: {id} does not found";
            return RedirectToAction(nameof(Error));
        }
        return View(book);
    }


    //public IActionResult ViewBook(Book book)
    //{
    //    if (book == null)
    //    {
    //        TempData["errorMessage"] = $"Book does not found in the Database.";
    //        return RedirectToAction(nameof(Error));
    //    }
    //    return View(book);
    //}

    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IActionResult> AddBook()
    {
        var genreSelectList = (await _genreService.GetGenres()).Select(genre => new SelectListItem
        {
            Text = genre.GenreName,
            Value = genre.Id.ToString(),
        });
        BookDTO bookToAdd = new() { GenreList = genreSelectList };
        return View(bookToAdd);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpPost]
    public async Task<IActionResult> AddBook(BookDTO bookToAdd)
    {
        var genreSelectList = (await _genreService.GetGenres()).Select(genre => new SelectListItem
        {
            Text = genre.GenreName,
            Value = genre.Id.ToString(),
        });
        bookToAdd.GenreList = genreSelectList;

        if (!ModelState.IsValid)
            return View(bookToAdd);

        try
        {
            if (bookToAdd.ImageFile != null)
            {
                if(bookToAdd.ImageFile.Length> 1 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Image file can not exceed 1 MB");
                }
                string[] allowedExtensions = [".jpeg",".jpg",".png"];
                string imageName=await _fileService.SaveFile(bookToAdd.ImageFile, allowedExtensions);
                bookToAdd.Image = imageName;
            }
            // manual mapping of BookDTO -> Book
            Book book = new()
            {
                Id = bookToAdd.Id,
                BookName = bookToAdd.BookName,
                AuthorName = bookToAdd.AuthorName,
                Image = bookToAdd.Image,
                GenreId = bookToAdd.GenreId,
                Price = bookToAdd.Price
            };
            await _bookService.AddBook(book);
            TempData["successMessage"] = "Book is added successfully";
            return RedirectToAction(nameof(AddBook));
        }
        catch (InvalidOperationException ex)
        {
            TempData["errorMessage"]= ex.Message;
            return View(bookToAdd);
        }
        catch (FileNotFoundException ex)
        {
            TempData["errorMessage"] = ex.Message;
            return View(bookToAdd);
        }
        catch (Exception ex)
        {
            TempData["errorMessage"] = "Error on saving data";
            return View(bookToAdd);
        }
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IActionResult> UpdateBook(int id)
    {
        var book = await _bookService.GetBookById(id);
        if(book==null)
        {
            TempData["errorMessage"] = $"Book with the id: {id} does not found";
            return RedirectToAction(nameof(Index));
        }
        var genreSelectList = (await _genreService.GetGenres()).Select(genre => new SelectListItem
        {
            Text = genre.GenreName,
            Value = genre.Id.ToString(),
            Selected=genre.Id==book.GenreId
        });
        BookDTO bookToUpdate = new() 
        { 
            GenreList = genreSelectList,
            BookName=book.BookName,
            AuthorName=book.AuthorName,
            GenreId=book.GenreId,
            Price=book.Price,
            Image=book.Image 
        };
        return View(bookToUpdate);
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpPost]
    public async Task<IActionResult> UpdateBook(BookDTO bookToUpdate)
    {
        var genreSelectList = (await _genreService.GetGenres()).Select(genre => new SelectListItem
        {
            Text = genre.GenreName,
            Value = genre.Id.ToString(),
            Selected=genre.Id==bookToUpdate.GenreId
        });
        bookToUpdate.GenreList = genreSelectList;

        if (!ModelState.IsValid)
            return View(bookToUpdate);

        try
        {
            string oldImage = "";
            if (bookToUpdate.ImageFile != null)
            {
                if (bookToUpdate.ImageFile.Length > 1 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Image file can not exceed 1 MB");
                }
                string[] allowedExtensions = [".jpeg", ".jpg", ".png"];
                string imageName = await _fileService.SaveFile(bookToUpdate.ImageFile, allowedExtensions);
                // hold the old image name. Because we will delete this image after updating the new
                oldImage = bookToUpdate.Image;
                bookToUpdate.Image = imageName;
            }
            // manual mapping of BookDTO -> Book
            Book book = new()
            {
                Id=bookToUpdate.Id,
                BookName = bookToUpdate.BookName,
                AuthorName = bookToUpdate.AuthorName,
                GenreId = bookToUpdate.GenreId,
                Price = bookToUpdate.Price,
                Image = bookToUpdate.Image
            };
            await _bookService.UpdateBook(book);
            // if image is updated, then delete it from the folder too
            if(!string.IsNullOrWhiteSpace(oldImage))
            {
                _fileService.DeleteFile(oldImage);
            }
            TempData["successMessage"] = "Book is updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["errorMessage"] = ex.Message;
            return View(bookToUpdate);
        }
        catch (FileNotFoundException ex)
        {
            TempData["errorMessage"] = ex.Message;
            return View(bookToUpdate);
        }
        catch (Exception ex)
        {
            TempData["errorMessage"] = "Error on saving data";
            return View(bookToUpdate);
        }
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IActionResult> DeleteBook(int id)
    {
        try
        {
            var book = await _bookService.GetBookById(id);
            if (book == null)
            {
                TempData["errorMessage"] = $"Book with the id: {id} does not found";
            }
            else
            {
                await _bookService.DeleteBook(book);
                if (!string.IsNullOrWhiteSpace(book.Image))
                {
                    _fileService.DeleteFile(book.Image);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["errorMessage"] = ex.Message;
        }
        catch (FileNotFoundException ex)
        {
            TempData["errorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["errorMessage"] = "Error on deleting the data";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = nameof(Roles.Admin))]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
