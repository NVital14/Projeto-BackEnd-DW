using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projeto.Data;
using Projeto.Models;

namespace Projeto.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reviews.Include(r => r.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
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
            //obter a lista de professores existentes na BD
            ViewData["UsersList"] = _context.Utilizadores.OrderBy(u => u.UserName).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Rating,IsShared,CategoryFK")] Reviews review, IFormFile? ImageReview, String[]userIdsList)
        {
            //verificar se o utilizador selecionou uma categoria
            if(review.CategoryFK == -1)
            {
                ModelState.AddModelError("", "Deve escolher uma categoria!");
                ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
                ViewData["UsersList"] = _context.Utilizadores.OrderBy(u => u.UserName).ToList();
                return View(review);
            }

            //verificar que foi introduzido o rating
            if(review.Rating == 0)
            {
                ModelState.AddModelError("", "Deve escolher a pontuação!");
                ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", review.CategoryFK);
                ViewData["UsersList"] = _context.Utilizadores.OrderBy(u => u.UserName).ToList();
                return View(review);
            }

            //verificar se houver colaboradores
            if (!userIdsList.IsNullOrEmpty())
            {
                var usersList = new List<Utilizadores>();
                foreach (var id in userIdsList)
                {
                    var user = _context.Utilizadores.FirstOrDefault(u => u.UserId == id);
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
                        ViewData["UsersList"] =_context.Utilizadores.OrderBy(u => u.UserName).ToList();
                        return View(review);
                    }
                }
            }
            //se não houver colaboradores na review, então guarda apenas o userId do utilizador que criou a review
            else
            {
                var userList = new List<Utilizadores>();
                var currentUserId = _userManager.GetUserId(User);
                var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
                userList.Add(util);
                review.Users = userList; 
            }

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
                        ViewData["UsersList"] = _context.Utilizadores.OrderBy(u => u.UserName).ToList();
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
            ViewData["UsersList"] = _context.Utilizadores.OrderBy(u => u.UserName).ToList();
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews.FindAsync(id);
            if (reviews == null)
            {
                return NotFound();
            }
            ViewData["CategoryFK"] = new SelectList(_context.Categories, "CategoryId", "Name", reviews.CategoryFK);
            ViewData["UsersList"] = _context.Utilizadores.OrderBy(u => u.UserName).ToList();
            return View(reviews);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,Title,Description,Rating,Image,CategoryFK, IsShared")] Reviews review, IFormFile? ImageReview)
        {
            //select * from Reviews where ReviewId = id
            Reviews r = _context.Reviews.AsNoTracking().FirstOrDefault(r => r.ReviewId == id);
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
                         *          1.1.2 - guarda a review sem alterar o campo da imagem
                         */

                     _context.Attach(review);
                    //tem imagem nova?
                    if (ImageReview != null)
                    {
                        //a review já tinha uma imagem?
                        if (r.Image != null)
                        {
                            //eliminar imagem antiga
                            var localizacaoImagem = Path.Combine(_webHostEnvironment.WebRootPath, "Imagens");
                            var oldImagePath = Path.Combine(localizacaoImagem, r.Image);
                            System.IO.File.Delete(oldImagePath);
                            
                        }
                        //guardar nova imagem
                        string imageName = generateImageName(ImageReview);
                        review.Image = imageName;
                        await saveImage(ImageReview, imageName);

                        //atualizar o campo imagem
                        _context.Entry(review).Property("Image").IsModified = true; 
                       
                    }
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
