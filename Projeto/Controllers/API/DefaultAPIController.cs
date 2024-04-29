using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Projeto.Data;
using Projeto.Models;

namespace Projeto.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefaultAPIController : ControllerBase
    {
        public ApplicationDbContext _context;

        public DefaultAPIController(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }
        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            return Ok("Olá");
        }
        [HttpGet]
        [Route("ola")]
        public ActionResult OlaNome(string nome)
        {
            return Ok("Olá" + nome);
        }

        [HttpGet]
        [Route("categories")] 
        public ActionResult GetCategories()
        {
            var category = new Categories
            {
                Name = "Filmes",
            };
            _context.Categories.Add(category);
            _context.SaveChanges();
            var list = _context.Categories.ToList();
            return Ok(list);
        }

        [HttpPost]
        [Route("createCategory")]
        public ActionResult createCategory()
        {
            var category = new Categories
            {
                Name = "Filmes",
            };
            _context.Categories.Add(category);
            _context.SaveChanges();
            var list = _context.Categories.ToList();
            return Ok(list);
        }
    }
}
