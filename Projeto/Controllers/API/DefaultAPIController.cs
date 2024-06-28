using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        public UserManager <IdentityUser> _userManager;
        public SignInManager<IdentityUser> _signInManager;
        public DefaultAPIController(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager)
        {
            _context = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
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


    }
}
