using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Projeto.Data;
using Projeto.Models;

namespace Projeto.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizadoresController : Controller
    {
        public ApplicationDbContext _context;
        public UserManager<IdentityUser> _userManager;
        public SignInManager<IdentityUser> _signInManager;
        public UtilizadoresController(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Cria utilizador
        /// </summary>
        /// <returns>Status Code</returns>
        [HttpPost]
        [Route("create-user")]
        public async Task<IActionResult> CreateUser()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(body); 
                if (data == null )
                    
                {
                    return BadRequest("Json Inválido, não enviou o email, a password ou o nome de utilizador");
                }
                if(data.email == null)
                {
                    return BadRequest("Json Inválido, não enviou o email, a password ou o nome de utilizador");
                }

                if(data.password == null)
                {
                    return BadRequest("Json Inválido, não enviou o email, a password ou o nome de utilizador");
                }
                if(data.userName == null)
                {
                    return BadRequest("Json Inválido, não enviou o email, a password ou o nome de utilizador");
                }
                try
                {
                    string email = (string)data.email;
                    string emailUpper = email.ToUpper();
                    var identityUser = new IdentityUser
                    {
                        Email = email,
                        UserName = (string)data.email,
                        NormalizedEmail = emailUpper,
                        Id = Guid.NewGuid().ToString(),
                        EmailConfirmed = true
                };

                    identityUser.PasswordHash = new PasswordHasher<IdentityUser>().HashPassword(identityUser, (string)data.password);

                    var result = await _userManager.CreateAsync(identityUser);
                    //await _context.SaveChangesAsync();

                    if(result.Succeeded)
                    {
                        try
                        {
                            // ***********************************
                            // guardar os dados do Utlizador
                            // ***********************************
                            var utilizador = new Utilizadores
                            {
                                UserId = identityUser.Id,
                                UserName = data.userName
                            };

                            // adicionar os dados do utilizador à BD
                            _context.Add(utilizador);
                            await _context.SaveChangesAsync();
                            // ***********************************

                            return Ok("Usuário criado com sucesso");
                        }
                        catch (Exception e)
                        {
                            return BadRequest($"Erro ao criar o utilizador: {e.Message}");
                        }
                    }
                    
                }
                catch
                {
                    return BadRequest("Não deu");
                }

                return BadRequest("Houve algum erro");
            }
            
        }

        /// <summary>
        /// Entrar na conta
        /// </summary>
        /// <returns>Status Code</returns>
        [HttpPost]
        [Route("sign-in-user")]
        public async Task<IActionResult> SignInUser()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(body);
                if (data == null)
                {
                    return BadRequest("Json Inválido, não enviou  dados");
                }
                if (data.email == null)
                {
                    return BadRequest("Json Inválido, não enviou o email");
                }

                if (data.password == null)
                {
                    return BadRequest("Json Inválido, não enviou a password ");
                }
                try
                {
                    var email = (string)data.email;
                    var password = (string)data.password;
                    IdentityUser user = _userManager.FindByEmailAsync(email).Result;

                    if (user != null)
                    {
                        PasswordVerificationResult passWorks = new PasswordHasher<IdentityUser>().VerifyHashedPassword(null, user.PasswordHash, password);
                        if (passWorks.Equals(PasswordVerificationResult.Success))
                        {
                            await _signInManager.SignInAsync(user, false);
                            return Ok("O utlizador entrou na conta");
                        }
                    }
                    return NotFound("O utilizador não foi encontrado");
                }
                catch (Exception e)
                {
                    return BadRequest($"Erro ao entrar na conta: {e.Message}");

                }
            }

        }

        /// <summary>
        /// Sair da conta
        /// </summary>
        /// <returns>Status Code</returns>
        [HttpPost]
        [Route("log-out-user")]
        public async Task<IActionResult> LogOutUser()
        {
            try
            {

                await _signInManager.SignOutAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
            
        }


        /// <summary>
        /// User
        /// </summary>
        /// <returns>Utilizador</returns>
        [HttpGet]
        [Route("user")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var util = await _context.Utilizadores.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                return Ok(util);
            }
            catch
            {
                return BadRequest();
            }

        }

        /// <summary>
        /// Verifica se o user está autenticado
        /// </summary>
        /// <returns>boolean</returns>
        [HttpGet]
        [Route("is-authenticaded")]
        public async Task<IActionResult> IsUserAuthenticated()
        {
            try
            {
                bool isAuthenticated = User.Identity.IsAuthenticated;

                if (isAuthenticated)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch
            {
                return BadRequest();
            }

        }

        /// <summary>
        /// Verifica se o user é admin
        /// </summary>
        /// <returns>boolean</returns>
        [HttpGet]
        [Route("is-admin")]
        public async Task<IActionResult> IsUserAdmin()
        {
            try
            {
                bool isAd = User.IsInRole("Admin");

                if (isAd)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch
            {
                return BadRequest();
            }

        }


        /// <summary>
        /// Via buscar todos os utilizadores
        /// </summary>
        /// <returns>lista de utilizadores</returns>
        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var users = await _context.Utilizadores.Where(u => u.UserId != currentUserId)
                                                         .OrderBy(u => u.UserName)
                                                         .ToListAsync();
                return Ok(users);
            }
            catch
            {
                return BadRequest();
            }

        }

    }
}

