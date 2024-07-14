using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Projeto.Data;
using Projeto.Models;

namespace Projeto.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class CategoriesController : ControllerBase
    {
        public ApplicationDbContext _context;
        public UserManager<IdentityUser> _userManager;
        public CategoriesController(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager)
        {
            _context = applicationDbContext;
            _userManager = userManager;
        }

        //GET
        /// <summary>
        /// Vai buscar todas as categorias
        /// </summary>
        /// <returns>Categorias</returns>
        [HttpGet]
        [Route("category")] //working
        public async Task<IActionResult> GetCategory()
        {
            try
            {

                var categoriesList = await _context.Categories.ToListAsync();
                return Ok(categoriesList);
            }
            catch
            {
                return BadRequest();
            }
        }

        //POST
        /// <summary>
        /// Criar uma nova categoria
        /// </summary>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>
        [HttpPost]
        [Route("create-category")] //working
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory()
        {
            try
            {

            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(body);

                if (data == null )
                {
                    return BadRequest("Json Inválido, não enviou o nome da categoria");
                }
                if(data.name == null)
                {
                    return BadRequest("Json Inválido, não enviou o nome da categoria");
                }

                var category = new Categories
                {
                    Name = (string)data.name,
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return Ok(category);
            }
            }
            catch
            {
                return BadRequest();
            }
        }

        //PUT
        /// <summary>
        /// Edita uma categoria
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>

        [HttpPut]
        [Route("edit-category/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditCategory([FromRoute] int id)
        {
            if (!CategoriesExists(id))
            {
                return NotFound();
            }
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(body);

                if (data == null || data.name == null || data.categoryId == null)
                {
                    return BadRequest("Não forneceu todos os parâmetros");
                }
                var category = new Categories
                {
                    CategoryId = (int)data.categoryId,
                    Name = (string)data.name,
                };

                if (id != category.CategoryId)
                {
                    return NotFound();
                }

                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw;

                }

                return Ok(category);
            }
        }

        //DELETE
        /// <summary>
        /// Elimina uma categoria
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>
        [HttpDelete]
        [Route("delete-category/{id}")] //working
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            if (!CategoriesExists(id))
            {
                return NotFound();
            }
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category != null)
                {
                    _context.Categories.Remove(category);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {

                return BadRequest();
            }

        }



        private bool CategoriesExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }


    }
}