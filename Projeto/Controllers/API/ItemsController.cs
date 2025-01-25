using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projeto.Data;
using Projeto.Models;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace Projeto.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : Controller
    {
        public ApplicationDbContext _context;
        public UserManager<IdentityUser> _userManager;
        /// <summary>
        /// objeto que contém os dados do Servidor
        /// </summary>
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ItemsController(ApplicationDbContext applicationDbContext, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = applicationDbContext;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        //POST
        /// <summary>
        /// Cria um item
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="price"></param>
        /// <param name="isChecked"></param>
        /// <param name="listId"></param>
        /// <param name="itemImage"></param>

        /// <returns>A review que foi criada</returns>
        [Authorize]
        [HttpPost]
        [Route("create-item/{listId}")]
        public async Task<IActionResult> CreateItem([FromRoute] int listId, [FromForm] string? itemName, [FromForm] IFormFile? itemImage, [FromForm] decimal? price, [FromForm] int amount, [FromForm] bool isChecked)
        {
            //verifica se a lista existe
            if (!ListExists(listId))
            {
                return BadRequest("Esta lista não existe!");
            }
            // Verifificar se os campos obrigatórios estão preenchidos
            if (itemName == null && itemImage == null)
            {
                return BadRequest("O item tem que ter o nome ou uma foto, não pode não ter nenhum deles!");
            }

            if (amount <= 0)
            {
                return BadRequest("A qauntidade tem que ser um valor maior que 0!");
            }

            var item = new Items
            {
                ItemName = itemName,
                IsChecked = isChecked,
                Price = price,
                ListFK = listId,
                Amount = amount
            };


            if (ModelState.IsValid)
            {
                // Guardar imagem se houver uma
                if (itemImage != null)
                {
                    //verifica se o tipo do ficheiro é válido
                    if (!(itemImage.ContentType == "image/png" || itemImage.ContentType == "image/jpeg"))
                    {
                        return BadRequest("Deve fornecer uma imagem válida!");
                    }

                    var imageName = GenerateImageName(itemImage);
                    item.Image = imageName;
                    await SaveImage(itemImage, imageName);
                }

                _context.Add(item);
                await _context.SaveChangesAsync();
                return Ok(item);
            }

            return BadRequest("Não deu!");

        }

        //GETS
        /// <summary>
        /// Vai buscar os items da lista
        /// </summary>
        /// <param name="listId">Id da lista</param>
        /// <returns>Uma lista de items</returns>
        [Authorize]
        [HttpGet]
        [Route("get-list-items/{listId}")] //working
        public async Task<IActionResult> GetItemsByListId([FromRoute] int listId)
        {
            //Este endpoint só é usado para ir buscar os items das listas

            List<Items> items;


            if (!ListExists(listId))
            {
                return NotFound("A lista não foi encontrada");
            }
            items = await _context.Items.Where(i => i.ListFK == listId).ToListAsync();

           
            return Ok(items);

        }
        //PUT
        [Authorize]
        [HttpPut]
        [Route("edit-item/{id}")]
        public async Task<IActionResult> EditItem([FromRoute] int id, [FromForm] string? itemName, [FromForm] IFormFile? itemImage, [FromForm] decimal? price, [FromForm] int amount, [FromForm] bool isChecked)
        {
            //verificar se o item existe
            if(!ItemExists(id))
            {
                return BadRequest("Não existe nenhum item com este id!");
            }
            // Verifificar se os campos obrigatórios estão preenchidos
            if (itemName == null && itemImage == null)
            {
                return BadRequest("O item tem que ter o nome ou uma foto, não pode não ter nenhum deles!");
            }

            if (amount <= 0)
            {
                return BadRequest("A qauntidade tem que ser um valor maior que 0!");
            }

            

            var item = await _context.Items.Where(i => i.ItemId == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    // atualizar os campos básicos
                    item.ItemName = itemName;
                    item.IsChecked = isChecked;
                    item.Price = price;
                    item.Amount = amount;

                    //tem imagem nova?
                    if (itemImage != null)
                    {
                        //a review já tinha uma imagem?
                        if (item.Image != null)
                        {
                            //eliminar imagem antiga
                            var localizacaoImagem = Path.Combine(_webHostEnvironment.WebRootPath, "Imagens");
                            var oldImagePath = Path.Combine(localizacaoImagem, item.Image);
                            System.IO.File.Delete(oldImagePath);

                        }
                        //guardar nova imagem
                        string imageName = GenerateImageName(itemImage);
                        item.Image = imageName;
                        await SaveImage(itemImage, imageName);

                    }
                    _context.Entry(item).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }


                catch (DbUpdateConcurrencyException e)
                {

                    return BadRequest("Houve um erro a editar o item");
                }
            }


            return Ok();
        }

        //DELETE
        /// <summary>
        /// Eliminar um item
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Estado</returns>
        [Authorize]
        [HttpDelete]
        [Route("delete-item/{id}")]
        public async Task<IActionResult> DeleteItem([FromRoute] int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            //verifica se o item existe
            if (!ItemExists(id))
            {
                return NotFound();
            }
            try
            {
                var item = await _context.Items.FindAsync(id);

            
                //se a review tiver imagem
                if (item.Image != null)
                {
                    //eliminar imagem 

                    var localizacaoImagem = Path.Combine(_webHostEnvironment.WebRootPath, "Imagens");
                    var oldImagePath = Path.Combine(localizacaoImagem, item.Image);
                    System.IO.File.Delete(oldImagePath);

                }
                if (item != null)
                {
                    _context.Items.Remove(item);
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

        /// <summary>
        /// Verifca a partir de um id se a lista existe
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool ListExists(int id)
        {
            return _context.Lists.Any(e => e.ListId == id);
        }

        /// <summary>
        /// Verifca a partir de um id se o item existe
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItemId == id);
        }

    }



}
