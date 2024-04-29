using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Categories
    {
        public Categories()
        {
            Reviews = new HashSet<Reviews>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        public String Name { get; set; }

        // relacionamento 1-N


        /// <summary>
        /// lista de reviews de cada categoria
        /// </summary>
        public ICollection<Reviews> Reviews { get; set; }
    }
}
