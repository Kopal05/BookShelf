using BookstaGram.Models;
using BookstaGram.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookstaGram.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;

        public HomeController(ILogger<HomeController> logger, IHomeService homeRepository)
        {
            _homeService = homeRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string sterm="",int genreId=0)
        {
            IEnumerable<Book> books = await _homeService.GetBooks(sterm, genreId);
            IEnumerable<Genre> genres = await _homeService.Genres();
            BookDisplayModel bookModel = new BookDisplayModel
            {
              Books=books,
              Genres=genres,
              STerm=sterm,
              GenreId=genreId
            };
            return View(bookModel);
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}