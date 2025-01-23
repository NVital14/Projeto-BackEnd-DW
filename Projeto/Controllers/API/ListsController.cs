using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Projeto.Data;
using Projeto.Models;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace Projeto.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListsController : Controller
    {
        public ApplicationDbContext _context;
        public UserManager<IdentityUser> _userManager;
        public ListsController(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager)
        {
            _context = applicationDbContext;
            _userManager = userManager;
        }

        //POST
        /// <summary>
        /// Criar uma nova lista
        /// </summary>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>
        [HttpPost]
        [Route("create-list")] //working
       // [Authorize]
        public async Task<IActionResult> CreateList()
        {
            var currentUserId = _userManager.GetUserId(User);
            //var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            var util = _context.Utilizadores.FirstOrDefault(u => u.Id == 1);
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    dynamic data = JsonConvert.DeserializeObject(body);

                    if (data == null)
                    {
                        return BadRequest("Json Inválido, não enviou o nome da lista");
                    }
                    if (data.name == null)
                    {
                        return BadRequest("Json Inválido, não enviou o nome da lista");
                    }
                    if (util == null)
                    {
                        return BadRequest("Tem que estar autenticado");
                    }

                    var list = new Lists
                    {
                        ListName = (string)data.name,
                        UtilizadorFK = util.Id
                };


                _context.Lists.Add(list);
                await _context.SaveChangesAsync();

                return Ok(list);

            }
            }
            catch {
            return BadRequest();
    }
        }

        //GET
        /// <summary>
        /// Vai buscar todas as listas com base no utilizador
        /// </summary>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>
        [HttpGet]
        [Route("get-lists")] //working
       // [Authorize]
        public async Task<IActionResult> GetLists()
        {
            //var util = _context.Utilizadores.FirstOrDefault(u => u.UserId == currentUserId);
            var util = _context.Utilizadores.FirstOrDefault(u => u.Id == 1);
            List<Lists> lists;
            try
            {
                if (util == null)
                {
                    return Forbid();
                }
                lists = await _context.Lists.Where(l => l.UtilizadorFK == util.Id).ToListAsync();

                return Ok(lists);
            }
            catch
            {
                return BadRequest();
            }
        }

        //PUT
        /// <summary>
        /// Edita uma lista
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>

        [HttpPut]
        [Route("edit-list/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditList([FromRoute] int id)
        {
            if (!ListExists(id))
            {
                return NotFound();
            }
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(body);

                if (data == null || data.name == null || data.listId == null)
                {
                    return BadRequest("Não forneceu todos os parâmetros");
                }
               

                var list = new Lists
                {
                    ListName = (string)data.name,
                    ListId = (int)data.listId,
                    UtilizadorFK = 1 //mudar depois para o user mesmo
                    };

                if (id != list.ListId)
                {
                    return NotFound();
                }

                try
                {
                    _context.Update(list);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw;

                }

                return Ok(list);
            }
        }
        //DELETE
        /// <summary>
        /// Elimina uma lista
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Mensagem de sucesso ou não sucesso</returns>
        [HttpDelete]
        [Route("delete-list/{id}")] //working
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteList([FromRoute] int id)
        {
            if (!ListExists(id))
            {
                return NotFound();
            }
            try
            {
                var list = await _context.Lists.FindAsync(id);
                if (list != null)
                {
                    _context.Lists.Remove(list);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {

                return BadRequest();
            }

        }
        private bool ListExists(int id)
        {
            return _context.Lists.Any(e => e.ListId == id);
        }

    }
}
