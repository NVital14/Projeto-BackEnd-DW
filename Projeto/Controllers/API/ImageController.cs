using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projeto.Data;
using System.Net.Mime;

namespace Projeto.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : Controller
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

        public ImageController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }



        [HttpGet("{imageName}")]
        public IActionResult GetImage([FromRoute]string imageName)
        {
            string _imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Imagens");
            var imageFilePath = Path.Combine(_imagePath, imageName);
            if (!System.IO.File.Exists(imageFilePath))
            {
                return NotFound("Image not found");
            }

            var fileExtension = Path.GetExtension(imageFilePath).ToLower();
            string mimeType = GetMimeType(fileExtension);

            var imageFileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            return File(imageFileStream, mimeType);
        }

        private string GetMimeType(string fileExtension)
        {
            return fileExtension switch
            {
                ".jpg" => MediaTypeNames.Image.Jpeg,
                ".jpeg" => MediaTypeNames.Image.Jpeg,
                ".png" => "image/png",
                ".gif" => MediaTypeNames.Image.Gif,
                _ => "application/octet-stream",
            };
        }
    }
}
