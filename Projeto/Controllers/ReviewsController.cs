using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projeto.Data;
using Projeto.Models;

namespace Projeto.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        /// <summary>
        /// referência à BD do projeto
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// objeto que contém os dados do Servidor
        /// </summary>
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// objeto para interagir com os dados da pessoa autenticada
        /// </summary>
        private readonly UserManager<IdentityUser> _userManager;




        public ReviewsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: Reviews
        [AllowAnonymous] // uma pessoa sem estar autenticada CONSEGUE aceder
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            if (util != null)
            {
                var applicationDbContext = _context.Reviews.Include(r => r.Category).Where(r => r.Users.Any(u => u.Id == util.Id));
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                // tratar o caso em que o utilizador não é encontrado
                return NotFound();
            }
        }

        // GET: Reviews/Details/5
        [AllowAnonymous] 
        public async Task<IActionResult> Details(int? id, string source)
        {
            //define o valor da source no ViewBag
            ViewBag.Source = source;
            if (id == null)
            {
                return NotFound();
            }


            var commentsList = _context.Comments
                             .Where(c => c.ReviewFK == id)
                             .Join(_context.Utilizadores,
                                   c => c.UtilizadorFK, // supondo que Comment tenha uma propriedade UserId que referencia Utilizadores
                                   u => u.Id,
                                   (c, u) => new Comments
                                   {
                                       CommentId = c.CommentId,
                                       Comment = c.Comment,
                                       Utilizador = u
                                   })
                             .OrderBy(c => c.CommentId)
                             .ToList();

            ViewData["CommentsList"] = commentsList;


            var reviews = await _context.Reviews
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (reviews == null)
            {
                return NotFound();
            }

            return View(reviews);
        }
        /// <summary>
        /// Método que guarda a imagem no disco rigído
        /// </summary>
        /// <param name="ImageReview">ficheiro da imagem</param>
        /// <param name="imageName">nome da imagem</param>
        /// <returns></returns>
        public async Task saveImage(IFormFile ImageReview, String imageName)
        {
           
                string localizacaoImagem = Path.Combine(_webHostEnvironment.WebRootPath, "Imagens");
                Console.WriteLine(localizacaoImagem);

                if (!Directory.Exists(localizacaoImagem))
                {
                    Directory.CreateDirectory(localizacaoImagem);
                }

                localizacaoImagem = Path.Combine(localizacaoImagem, imageName);

                using var stream = new FileStream(localizacaoImagem, FileMode.Create);
                await ImageReview.CopyToAsync(stream);
             
        }

        /// <summary>
        /// Método que gera nome da imagem
        /// </summary>
        /// <param name="ImageReview"> Ficheiro da imagem</param>
        /// <returns>Retorna uma string com o nome da imagem</returns>
        public String generateImageName(IFormFile ImageReview)
        {
            Guid g = Guid.NewGuid();
            var imageName = g.ToString() + Path.GetExtension(ImageReview.FileName).ToLowerInvariant();
            return imageName;
        }
        // GET: Reviews/Create
        public IActionResult Create()
        {
            ViewData["CategoryFK"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "CategoryId", "Name");
            //obter a lista de utilizadores existentes na BD, com excessão do utilizador atual
            var currentUserId = _userManager.GetUserId(User);
            ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                         .OrderBy(u => u.UserName)
                                                         .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Rating,IsShared,CategoryFK")] Reviews review, IFormFile? ImageReview, int[]userIdsList)
        {
            var currentUserId = _userManager.GetUserId(User);
            var usersList = new List<Utilizadores>();
            //verificar se o utilizador selecionou uma categoria
            if (review.CategoryFK == -1)
            {
                ModelState.AddModelError("", "Deve escolher uma categoria!");
                ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
                ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                             .OrderBy(u => u.UserName)
                                                             .ToList();
                return View(review);
            }

            //verificar que foi introduzido o rating
            if(review.Rating == 0)
            {
                ModelState.AddModelError("", "Deve escolher a pontuação!");
                ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
                ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                             .OrderBy(u => u.UserName)
                                                             .ToList();
                return View(review);
            }

            //verificar se há colaboradores
            if (!userIdsList.IsNullOrEmpty())
            {
                //se sim, guarda os colaboradores 
                foreach (var id in userIdsList)
                {
                    var user = _context.Utilizadores.FirstOrDefault(u => u.Id == id);
                    if (user != null)
                    {
                        usersList.Add(user);
                    }

                    if (userIdsList != null)
                    {
                        review.Users = usersList;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Houve um erro a guardar os colaboradores !");
                        ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
                        ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                             .OrderBy(u => u.UserName)
                                                             .ToList();
                        return View(review);
                    }
                }
            }
            // guarda o userId do utilizador que criou a review
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            usersList.Add(util);
            review.Users = usersList; 
            

          /* Guardar a imagem no disco rígido do Servidor
          * Algoritmo
          * 1- há ficheiro?
          *    1.1 - não
          *          guarda a review sem imagem
          *    1.2 - sim
          *          Será imagem (JPG,JPEG,PNG)?
          *          1.2.1 - não
          *                  envia mensagem de erro
          *          1.2.2 - sim
          *                  - determinar o nome da imagem - chamar função generateImageName()
          *                  - guardar esse nome na BD
          *                  - guardar a imagem - chamar a função saveImage()
          */

            if (ModelState.IsValid)
            {
                string imageName = "";
                bool hasImage = false;

                if (ImageReview != null)
                {
                    if (!(ImageReview.ContentType == "image/png" || ImageReview.ContentType == "image/jpeg"))
                    {
                        ModelState.AddModelError("", "Deve fornecer uma imagem!");
                        ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
                        ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                             .OrderBy(u => u.UserName)
                                                             .ToList();
                        return View(review);
                    }
                    else
                    {
                        hasImage = true;
                        imageName = generateImageName(ImageReview);
                        review.Image = imageName;
                    }
                }

                _context.Add(review);
                await _context.SaveChangesAsync();

                if (hasImage)
                {
                  await saveImage(ImageReview, imageName);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
            ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                             .OrderBy(u => u.UserName)
                                                             .ToList();
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //obter o userId do utilizador atual
            var currentUserId = _userManager.GetUserId(User);
            //obter o id da tabela Utilizadores do utilizador atual
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
            ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                         .OrderBy(u => u.UserName)
                                                       .ToList();
            ViewData["SelectedUserIds"] = _context.GetReviewUsers(id, util.Id);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,Title,Description,Rating,Image,CategoryFK, IsShared")] Reviews review, IFormFile? ImageReview, int[]userIdsList)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            var usersList = new List<Utilizadores>();
 
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    /*Atualizar review
                         * 1 - Verificar se tem uma imagem nova
                         *      1.1 - se sim
                         *          1.1.1 - verificar se havia uma imagem antiga
                         *              1.1.1.1 - se sim, apaga a imagem anterior
                         *          1.1.2 - gera um nome e guarda a nova imagem
                         *      1.2 - se não
                         *          1.2.1 - verificar se os colaboradores foram alterados
                         *                1.2.1.1 - seguir algoritmo de atualizar os users
                         *          1.2.2 - guarda a review sem alterar o campo da imagem e dos users
                         */

                     _context.Attach(review);
                    //tem imagem nova?
                    if (ImageReview != null)
                    {
                        //a review já tinha uma imagem?
                        if (review.Image != null)
                        {
                            //eliminar imagem antiga
                            var localizacaoImagem = Path.Combine(_webHostEnvironment.WebRootPath, "Imagens");
                            var oldImagePath = Path.Combine(localizacaoImagem, review.Image);
                            System.IO.File.Delete(oldImagePath);
                            
                        }
                        //guardar nova imagem
                        string imageName = generateImageName(ImageReview);
                        review.Image = imageName;
                        await saveImage(ImageReview, imageName);

                        //atualizar o campo imagem
                        _context.Entry(review).Property("Image").IsModified = true; 
                       
                    }

                    /*Algoritmo atualizar users(colaboradores)
                     * 1 - Foram excluídos colaboradores?
                     *  1.1 - sim, então eliminar esses users como colaboradores
                     * 2 - Foram adicionados colaboradores?
                     *  2.1 - Sim
                     *      2.1.1 - ir buscar o utilizador com id correspondente ao do novo colaborador
                     *      2.1.2 - adicionar o utilizador a uma lista dos utilizadores
                     *      2.1.3 - a lista dos utilizadores não está fazia?
                     *          2.2.1 - sim, guarda os utilizadores
                     *          2.2.2 - não, manda uma mensagem de erro
                     */
                    //lista de utilizadores da review antes de ser editada
                    List <int> usersIdSaved = _context.GetReviewUsers(review.ReviewId, util.Id);
                    //idsToDelete - são os users que foram guardados na review, mas que deixaram de ser colaboradores
                    IEnumerable<int> idsToDelete = usersIdSaved.Except(userIdsList);
                    //idsToSave - são os users que não eram colboradores quando a review foi criada, mas passaram a ser
                    IEnumerable<int> idsToSave = userIdsList.Except(usersIdSaved);

                    //há algum user para eliminar?
                    if (idsToDelete.Any())
                    {
                        //eliminar os colaboradores guardados antes da review ser editada
                        foreach (int usId in idsToDelete)
                        {
                            _context.DeleteColaborator(review.ReviewId, usId);
                        }
                    }

                    //há algum user para guardar?
                    if (idsToSave.Any())
                    {
                        foreach (int usId in idsToSave)
                        {
                            //ir buscar o utilizador com o id igual ao do id do novo colaborador
                            var user = _context.Utilizadores.FirstOrDefault(u => u.Id == usId);
                            if (user != null)
                            {
                                usersList.Add(user);
                            }

                            if (userIdsList != null)
                            {
                                //guarda a lista de colaboradores na lista de users da review
                                review.Users = usersList;
                            }
                            else
                            {
                                ModelState.AddModelError("", "Houve um erro a guardar os colaboradores !");
                                ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
                                ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                                     .OrderBy(u => u.UserName)
                                                                     .ToList();
                                return View(review);
                            }
                        }
                    }
                    ////comparar a lista de utilizadores da review já guardados, com a lista de agora
                    //if (!(userIdsList.All(id => usersIdSaved.Contains(id))))
                    //{
                    //    //eliminar os colaboradores guarados antes da review ser editada
                    //    foreach(int usId in usersIdSaved)
                    //    {
                    //        _context.DeleteColaborator(review.ReviewId, usId);
                    //    }
                    //    //faz uma nova lista de colaboradores 
                    //    foreach (int i in userIdsList)
                    //    {
                    //        var user = _context.Utilizadores.FirstOrDefault(u => u.Id == i);
                    //        if (user != null)
                    //        {
                    //            usersList.Add(user);
                    //        }

                    //        if (userIdsList != null)
                    //        {
                    //            //guarda a lista de colaboradores na lista de users da review
                    //            review.Users = usersList;
                    //        }
                    //        else
                    //        {
                    //            ModelState.AddModelError("", "Houve um erro a guardar os colaboradores !");
                    //            ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
                    //            ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                    //                                                 .OrderBy(u => u.UserName)
                    //                                                 .ToList();
                    //            return View(review);
                    //        }
                    //    }

                    //}
                    

                    //atualizar os restantes campos
                    _context.Entry(review).Property("Title").IsModified = true;
                     _context.Entry(review).Property("Description").IsModified = true;
                     _context.Entry(review).Property("Rating").IsModified = true;
                     _context.Entry(review).Property("CategoryFK").IsModified = true;
                     _context.Entry(review).Property("IsShared").IsModified = true;
                     await _context.SaveChangesAsync();

                    //_context.Update(review);
                }
                catch (DbUpdateConcurrencyException e)
                {
                    if (!ReviewsExists(review.ReviewId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        // Adiciona a mensagem de erro ao ModelState
                        ModelState.AddModelError(string.Empty, $"Erro ao salvar a revisão: {e.Message}");
                        
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
            ViewData["UsersList"] = _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                         .OrderBy(u => u.UserName)
                                                         .ToList();
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (reviews == null)
            {
                return NotFound();
            }

            return View(reviews);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reviews = await _context.Reviews.FindAsync(id);
            if (reviews != null)
            {
                _context.Reviews.Remove(reviews);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewsExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}
