using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Projeto.Data;
using Projeto.Models;

namespace Projeto.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : Controller
    {
        /// <summary>
        /// referência à BD do projeto
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// objeto para interagir com os dados da pessoa autenticada
        /// </summary>
        private readonly UserManager<IdentityUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Cria um comentário
        /// </summary>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>
        [HttpPost]
        [Route("create-comment/{revId}")] //working
        [Authorize]
        public async Task<IActionResult> SaveComments([FromRoute]int revId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            if(util == null)
            {
                return Forbid("Para fazer um comentário tem que estar na sua conta!");
            }



            if(!(_context.Reviews.Any(e => e.ReviewId == revId)))
            {
                return NotFound("Não existe essa review!");
            }
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(body);

                if (data == null || data.comment == null)
                {
                    return BadRequest("Json Inválido, não enviou o nome da categoria");
                }

                try
                {
                    Reviews r = _context.Reviews.AsNoTracking().FirstOrDefault(r => r.ReviewId == revId);

                    var c = new Comments
                    {
                        Comment = data.comment,
                        ReviewFK = revId,
                        UtilizadorFK = util.Id
                    };


                    r.Comments.Add(c);
                    _context.Comments.Add(c);
                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception)
                {

                    return BadRequest();
                }
            }

        }

    }
}
