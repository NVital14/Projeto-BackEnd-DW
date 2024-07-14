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
        /// objeto que contém os dados do Servidor
        /// </summary>
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ImageController(IWebHostEnvironment webHostEnvironment)
        {
        
            _webHostEnvironment = webHostEnvironment;
  
        }


        /// <summary>
        /// Vai buscar a imagem armazenada no servidor
        /// </summary>
        /// <param name="imageName">nome da imagem</param>
        /// <returns>ficheiro da imagem</returns>
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

        //vai buscar a extensão da imagem
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
