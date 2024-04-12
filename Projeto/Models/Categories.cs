using System.ComponentModel.DataAnnotations;

namespace Projeto.Models
{
    public class Categories
    {
        [Key]
        public int CategorieId { get; set; }

        public String Name { get; set; }
    }
}
