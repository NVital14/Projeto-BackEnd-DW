using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Projeto.Data;
using Projeto.Models;
using System.Drawing.Printing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Projeto.Controllers.API
{
        [Route("api/[controller]")]
        [ApiController]
    public class FavoritesController : Controller
    {
        /// <summary>
        /// referência à BD do projeto
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// objeto para interagir com os dados da pessoa autenticada
        /// </summary>
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// vai buscar os favoritos
        /// </summary>
        /// <returns>favoritos</returns>
        [HttpGet]
        [Route("get-favorites")] //working
        [Authorize]
        public async Task<IActionResult> GetFavorites()
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            if (util == null)
            {
                return BadRequest();
            }
            try
            {

            var favorites = await _context.Favorites.Where(f => f.UtilizadorFK == util.Id).ToListAsync();

            if(favorites == null)
                {
                    return Ok(null);
                }
            return Ok(favorites);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }


        /// <summary>
        /// Cria um comentário
        /// </summary>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>
        [HttpPost]
        [Route("add-favorite/{revId}")] //working
        [Authorize]
        public async Task<IActionResult> AddFavorite([FromRoute] int revId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            if (util == null) {
                return BadRequest();
            }
            if (!(_context.Reviews.Any(e => e.ReviewId == revId)))
            {
                return NotFound("Não existe essa review!");
            }

            try
            {
                Reviews r = _context.Reviews.AsNoTracking().FirstOrDefault(r => r.ReviewId == revId);
                var f = new Favorites
                {
                    UtilizadorFK = util.Id,
                    ReviewFK = revId,
                    Utilizador = util
                };


                r.Favorites.Add(f);
                _context.Favorites.Add(f);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        /// <summary>
        /// Apaga um favorito
        /// </summary>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>
        [HttpDelete]
        [Route("delete-favorite/{revId}")] //working
        [Authorize]
        public async Task<IActionResult> DeleteFavorite([FromRoute] int revId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            if (util == null)
            {
                return BadRequest();
            }
            if (!(_context.Reviews.Any(e => e.ReviewId == revId)))
            {
                return NotFound("Não existe essa review!");
            }

            try
            {
                Favorites f = await _context.Favorites.Where(f => f.ReviewFK == revId && f.UtilizadorFK == util.Id).FirstOrDefaultAsync();

                if(f != null)
                {
                    _context.Favorites.Remove(f);
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
