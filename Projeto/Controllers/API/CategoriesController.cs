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
    [Authorize(Roles = "Admin")]
    public class CategoriesController : ControllerBase
    {
        public ApplicationDbContext _context;
        public UserManager <IdentityUser> _userManager;
        public SignInManager<IdentityUser> _signInManager;
        public CategoriesController(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager)
        {
            _context = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpGet]
        [Route("category")] //working
        public async Task<IActionResult> GetCategory()
        {
            var categoriesList = await _context.Categories.ToListAsync();
            return Ok(categoriesList);
        }

        [HttpGet]
        [Route("category-id/{id}")] //working
        public async Task<IActionResult> GetCategoryById([FromRoute] int id)
        {

            if (!CategoriesExists(id))
            {
                return NotFound("A Categoria não foi encontrada");
            }
            var category = await _context.Categories
                                          .FirstOrDefaultAsync(c => c.CategoryId == id);
            return Ok(category);

        }

        [HttpPost]
        [Route("create-category")] //working
        public async Task<IActionResult> CreateCategory()
        {

            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(body);

                if (data == null || data.name == null)
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

        [HttpPut]
        [Route("edit-category/{id}")]
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

        [HttpDelete]
        [Route("delete-category/{id}")] //working
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

        [HttpPost]
        [Route("signInUser")]
        public async Task<ActionResult> SignInUtilizadorAsync([FromQuery] string email, [FromQuery] string password)
        {
            /* IdentityUser identityUser = new IdentityUser();
             identityUser.UserName = "aluno24872@ipt.pt";
             identityUser.Email = "aluno24872@ipt.pt";

             identityUser.NormalizedUserName = identityUser.UserName.ToUpper();
             identityUser.NormalizedEmail = identityUser.UserName.ToUpper();

             identityUser.PasswordHash = null;
             identityUser.Id = Guid.NewGuid().ToString();


             _userManager.CreateAsync(identityUser);

             _context.SaveChanges();*/

            try
            {
                IdentityUser user = _userManager.FindByEmailAsync(email).Result;

                if (user != null)
                {
                    PasswordVerificationResult passWorks = new PasswordHasher<IdentityUser>().VerifyHashedPassword(null, user.PasswordHash, password);
                    if (passWorks.Equals(PasswordVerificationResult.Success))
                    {
                        await _signInManager.SignInAsync(user, false);
                    }
                }
            }
            catch
            {

            }
            

            return Ok("ola");
        }

        private bool CategoriesExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }


    }
}
