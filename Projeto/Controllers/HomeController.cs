using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projeto.Data;
using Projeto.Models;
using System.Diagnostics;

namespace Projeto.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// refer�ncia � BD do projeto
        /// </summary>
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            ViewData["CategoriesList"] = await _context.Categories.AsNoTracking().OrderBy(u => u.Name).ToListAsync();

            if(categoryId == null)
            {
                var applicationDbContext = _context.Reviews.Include(r => r.Category).Where(r => r.IsShared == true);
                var reviews = await applicationDbContext.ToListAsync();
                return View(reviews);
            }
            else
            {
                //filtra as reviews por categoria
                var applicationDbContext = _context.Reviews.Include(r => r.Category).Where(r => r.IsShared == true && r.Category.CategoryId == categoryId);
                var reviews = await applicationDbContext.ToListAsync();
                ViewData["SelectedCategoryId"] = categoryId; // Passa o ID da categoria selecionada para a view
                return View(reviews);
            }

        }

        public IActionResult Privacy()
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
