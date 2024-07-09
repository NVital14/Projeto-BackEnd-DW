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
        public async Task<IActionResult> GetComments([FromRoute]int revId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
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

                    if (util == null)
                    {
                        //throw new Exception("Utilizador não encontrado.");
                        return Json(new { success = false, message = "O utilizador não está registado, não pode inserir comentários" });
                    }
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

                    return Json(new { success = true, message = "Comentário enviado com sucesso!", userName = util.UserName });
                }
                catch (Exception)
                {

                    return Json(new { success = false, message = "Ocorreu um erro ao enviar o comentário. Por favor, tente novamente mais tarde." });
                }
            }

        }

    }
}
