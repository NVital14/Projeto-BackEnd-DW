 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto.Data;
using Projeto.Models;

namespace Projeto.Controllers
{
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

        // GET: Coments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comments.Include(c => c.ReviewFK).Include(c => c.UtilizadorFK);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Coments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coments = await _context.Comments
                .Include(c => c.ReviewFK)
                .Include(c => c.UtilizadorFK)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (coments == null)
            {
                return NotFound();
            }

            return View(coments);
        }

        [Authorize]
        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["ReviewId"] = new SelectList(_context.Reviews, "ReviewId", "Description");
            ViewData["UserId"] = new SelectList(_context.Utilizadores, "Id", "Id");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comments coment, String c, int revId)
        {
            try
                {
                var currentUserId = _userManager.GetUserId(User);
                var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);

                if (util == null)
                {
                    //throw new Exception("Utilizador não encontrado.");
                    return Json(new { success = false, message = "O utilizador não está registado, não pode inserir comentários" });
                }
                //vai buscar a review em que foi feito o comentário
                Reviews r = _context.Reviews.AsNoTracking().FirstOrDefault(r => r.ReviewId == revId);
                coment.Comment = c;
                coment.ReviewFK = revId;

                coment.UtilizadorFK = util.Id;

                //adiona à lista dos comments, na review, o novo comentário
                r.Comments.Add(coment);
                _context.Comments.Add(coment);
                _context.SaveChanges();

                return Json(new { success = true, message = "Comentário enviado com sucesso!", userName = util.UserName});
            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Ocorreu um erro ao enviar o comentário. Por favor, tente novamente mais tarde." });
            }
        }

        // GET: Coments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coments = await _context.Comments.FindAsync(id);
            if (coments == null)
            {
                return NotFound();
            }
            ViewData["ReviewId"] = new SelectList(_context.Reviews, "ReviewId", "Description", coments.Review);
            ViewData["UserId"] = new SelectList(_context.Utilizadores, "Id", "Id", coments.Utilizador);
            return View(coments);
        }

        // POST: Coments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComentId,Coment,UserId,ReviewId")] Comments coments)
        {
            if (id != coments.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coments);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComentsExists(coments.CommentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReviewId"] = new SelectList(_context.Reviews, "ReviewId", "Description", coments.Review);
            ViewData["UserId"] = new SelectList(_context.Utilizadores, "Id", "Id", coments.Utilizador);
            return View(coments);
        }

        // GET: Coments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coments = await _context.Comments
                .Include(c => c.ReviewFK)
                .Include(c => c.UtilizadorFK)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (coments == null)
            {
                return NotFound();
            }

            return View(coments);
        }

        // POST: Coments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coments = await _context.Comments.FindAsync(id);
            if (coments != null)
            {
                _context.Comments.Remove(coments);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComentsExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
