using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Projeto.Data;
using Projeto.Models;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.Intrinsics.X86;

namespace Projeto.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
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

        //GETS
        /// <summary>
        /// Vai buscar apenas uma review com base no id
        /// </summary>
        /// <param name="id">Id da review</param>
        /// <returns>Uma review</returns>
        [HttpGet]
        [Route("review-id-details/{id}")] //working
        public async Task<IActionResult> GetReviewById([FromRoute] int id)
        {
            //Este endpoint só é usado para ir buscar reviews, para ver os detalhes da review
            
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);

               
            
            if (!ReviewExists(id))
            {
                return NotFound("A Review não foi encontrada");
            }
            var review = await _context.Reviews
                                          .FirstOrDefaultAsync(c => c.ReviewId == id);

            //um utilizador que tenha a permissão de admin tem acesso a todas as reviews
            if (!User.IsInRole("Admin"))
            {
                //se o utilizador for null e a review não estiver partilhada, não pode aceder
                if (util == null && review.IsShared == false) 
                {
                    return Forbid("Acesso Negado!");

                }
                //se a review não estiver partilhada, então, só os colaboradores podem ver os seus detalhes
                if (review.IsShared == false)
                    {
                        //todos os utilizadores associado à receita
                        var reviewUsers = _context.GetReviewUsers(id, util.Id, false);

                        //só um utilizador que seja colaborador da review é que pode mexer na página de editar
                        if (!reviewUsers.Contains(util.Id))
                        {
                            return Forbid("Acesso Negado!");
                        }
                    }
                
            }
            var comments = await _context.Comments.Where(c => c.ReviewFK == id).ToListAsync();
            foreach (var c in comments)
            {
                var u = await _context.Utilizadores.Where(u => u.Id == c.UtilizadorFK).FirstOrDefaultAsync();
                c.Utilizador = u;
            }
            var category = await _context.Categories.Where(c => c.CategoryId == review.CategoryFK).FirstOrDefaultAsync();
            review.Comments = comments;
            review.Category = category;
            return Ok(review);

        }



        /// <summary>
        /// Vai buscar as reviews paginadas
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns>As reviews por página</returns>
        [HttpGet]
        [Route("reviews-paginated")]
        public async Task<IActionResult> GetReviewsPaginated([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] bool byUser)
        {

            List<Reviews> reviews;
            var totalRecords = 0;
            try
            {

                if (!byUser)
                {
                    reviews = await _context.Reviews
                                           .Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToListAsync();

                totalRecords = await _context.Reviews.CountAsync();
                }
                else
                {
                    var currentUserId = _userManager.GetUserId(User);
                    var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);

                    if (util == null)
                    {
                        return Forbid();
                    }
                    reviews = await _context.Reviews
                                     .Include(r => r.Users) // Inclui os users
                                     .Where(r => r.Users.Any(u => u.Id == util.Id))
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();
                    totalRecords = await _context.Reviews.Where(r => r.Users.Any(u => u.Id == util.Id)).CountAsync();
                }

                foreach (var r in reviews)
            {
                var comments = await _context.Comments.Where(c => c.ReviewFK == r.ReviewId).ToListAsync();

                foreach (var c in comments)
                {
                    var u = await _context.Utilizadores.Where(u => u.Id == c.UtilizadorFK).FirstOrDefaultAsync();
                    c.Utilizador = u;
                }
                var category = await _context.Categories.Where(c => c.CategoryId == r.CategoryFK).FirstOrDefaultAsync();
                r.Comments = comments;
                r.Category = category;
            }
            var response = new
            {
                TotalRecords = totalRecords,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                Reviews = reviews
            };


            return Ok(response);
            }
            catch
            {
                return BadRequest();
            }
        }



        /// <summary>
        /// Vai buscar as reviews favoritas paginadas
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns>As reviews por página</returns>
        [HttpGet]
        [Route("favorites-paginated")]
        public async Task<IActionResult> GetFavoriteReviewsPaginated([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);

            List<Reviews> reviews;
            var totalRecords = 0;
            try
            {
            var favoriteReviewIds = await _context.Favorites.Where(f => f.UtilizadorFK == util.Id).Select(f => f.ReviewFK).ToListAsync();
                reviews = await _context.Reviews.Where(r => favoriteReviewIds.Contains(r.ReviewId))
                                          .Skip((pageNumber - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

                totalRecords = await _context.Reviews.Where(r => favoriteReviewIds.Contains(r.ReviewId)).CountAsync();

                foreach (var r in reviews)
                {
                    var comments = await _context.Comments.Where(c => c.ReviewFK == r.ReviewId).ToListAsync();

                    foreach (var c in comments)
                    {
                        var u = await _context.Utilizadores.Where(u => u.Id == c.UtilizadorFK).FirstOrDefaultAsync();
                        c.Utilizador = u;
                    }
                    var category = await _context.Categories.Where(c => c.CategoryId == r.CategoryFK).FirstOrDefaultAsync();
                    r.Comments = comments;
                    r.Category = category;
                }
                var response = new
                {
                    TotalRecords = totalRecords,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    Reviews = reviews
                };


                return Ok(response);
            }
            catch
            {
                return BadRequest();
            }
            
               
        }

        //POST
        /// <summary>
        /// Cria uma review
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="rating"></param>
        /// <param name="isShared"></param>
        /// <param name="categoryFK"></param>
        /// <param name="imageReview"></param>
        /// <param name="userIdsList"></param>
        /// <returns>A review que foi criada</returns>
        [Authorize]
        [HttpPost]
        [Route("create-review")] 
        public async Task<IActionResult> CreateReview([FromForm] string title,[FromForm] string description, [FromForm] int rating,[FromForm] bool isShared,[FromForm] int categoryFK,[FromForm] IFormFile? imageReview, [FromForm] int[]? userIdsList)
        {

            var currentUserId = _userManager.GetUserId(User);
            var usersList = new List<Utilizadores>();

            // Verifificar se os campos obrigatórios estão preenchidos
            if (title == null || categoryFK == -1 || rating == 0 || description == null)
            {
                return BadRequest("O título, a avaliação, a categoria e a descrição são de preenchimento obrigatório!");
            }


            // Verificar se há colaboradores
            if (!userIdsList.IsNullOrEmpty())
            {
                foreach (var id in userIdsList)
                {
                    var user = _context.Utilizadores.FirstOrDefault(u => u.Id == id);
                    if (user != null)
                    {
                        usersList.Add(user);
                    }
                }
            }
            //Adicionar o utilizador atual à lista de utilizadores da review
            var currentUser = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            usersList.Add(currentUser);


            var review = new Reviews
            {
                Title = title,
                Description = description,
                Rating = rating,
                IsShared = isShared,
                CategoryFK = categoryFK,
                Users = usersList
            };


            if (ModelState.IsValid)
            {
                // Guardar imagem se houver uma
                if (imageReview != null)
                {
                    //verifica se o tipo do ficheiro é válido
                    if (!(imageReview.ContentType == "image/png" || imageReview.ContentType == "image/jpeg"))
                    {
                        return BadRequest("Deve fornecer uma imagem válida!");
                    }

                    var imageName = GenerateImageName(imageReview);
                    review.Image = imageName;
                    await SaveImage(imageReview, imageName);
                }

                _context.Add(review);
                await _context.SaveChangesAsync();
                return Ok(review);
            }

            return BadRequest("Não deu!");

        }

        //PUT
        [Authorize]
        [HttpPut]
        [Route("edit-review/{id}")]
        public async Task<IActionResult> EditReview([FromRoute] int id, [FromForm] string title, [FromForm] string description, [FromForm] int rating, [FromForm] bool isShared, [FromForm] int categoryFK, [FromForm] IFormFile? imageReview, [FromForm] int[]? userIdsList)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            var usersList = new List<Utilizadores>();

            // Verifificar se os campos obrigatórios estão preenchidos
            if (title == null || categoryFK == -1 || rating == 0 || description == null)
            {
                return BadRequest("O título, a avaliação, a categoria e a descrição são de preenchimento obrigatório!");
            }
            if (!ReviewExists(id))
            {
                return NotFound();
            }

            var review = await _context.Reviews.Where(r => r.ReviewId == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    // atualizar os campos básicos
                    review.Title = title;
                    review.Description = description;
                    review.Rating = rating;
                    review.Image = review.Image;
                    review.IsShared = isShared;
                    review.CategoryFK = categoryFK;

                    //tem imagem nova?
                    if (imageReview != null)
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
                        string imageName = GenerateImageName(imageReview);
                        review.Image = imageName;
                        await SaveImage(imageReview, imageName);

                        //atualizar o campo imagem
                        //_context.Entry(review).Property("Image").IsModified = true;

                    }

                    /***************** atualizar users(colaboradores) ***************/
                    //lista de utilizadores da review antes de ser editada
                    List<int> usersIdSaved = _context.GetReviewUsers(review.ReviewId, util.Id, true);
                    //List<int> usersIdSaved = _context.GetReviewUsers(review.ReviewId, util.Id, true);
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
                            _context.DeleteCollaborator(review.ReviewId, usId);
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
                                return BadRequest("Houve um erro a guardar os colaboradores");
                            }

                        }
                    }
                     _context.Entry(review).State = EntityState.Modified;
                     await _context.SaveChangesAsync();
                }


                catch (DbUpdateConcurrencyException e)
                {

                    return BadRequest("Houve um erro a editar a review");


                }
            }


            return Ok();
        }

        //DELETE
        /// <summary>
        /// Elimina uma review
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Estado</returns>
        [HttpDelete]
        [Route("delete-review/{id}")] 
        public async Task<IActionResult> DeleteReview([FromRoute] int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            //verifica se essa review existe
            if (!ReviewExists(id))
            {
                return NotFound();
            }
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                var reviewUsers = _context.GetReviewUsers(id, util.Id, false);

                //se o utilizador que está a apagar a review, não estiver nos utilizadores que têm acesso à review, então não pode realizar a ação
                if(!reviewUsers.Contains(util.Id)) {
                    return Forbid("Não pode realizar esta ação!");
                }

                //se a review tiver imagem
                if (review.Image != null)
                {
                    //eliminar imagem 
                    
                    var localizacaoImagem = Path.Combine(_webHostEnvironment.WebRootPath, "Imagens");
                    var oldImagePath = Path.Combine(localizacaoImagem, review.Image);
                    System.IO.File.Delete(oldImagePath);

                }
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {

                return BadRequest();
            }

        }
        /// <summary>
        /// Método que verifica a existência de uma review
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True ou false</returns>
        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }

        

        /// <summary>
        /// Método que guarda a imagem no disco rigído
        /// </summary>
        /// <param name="ImageReview">ficheiro da imagem</param>
        /// <param name="imageName">nome da imagem</param>
        /// <returns></returns>
        public async Task SaveImage(IFormFile ImageReview, String imageName)
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
        public String GenerateImageName(IFormFile ImageReview)
        {
            Guid g = Guid.NewGuid();
            var imageName = g.ToString() + Path.GetExtension(ImageReview.FileName).ToLowerInvariant();
            return imageName;
        }

    }
}
