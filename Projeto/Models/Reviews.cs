using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Reviews
    {
        public Reviews() {
            Users = new HashSet<Users>();
        }
        [Key]
        public int ReviewId { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public String Image { get; set; }

        public int Rating { get; set;}

        public Boolean IsShared { get; set; }

        //relacionamento 1-N
        [ForeignKey(nameof(CategoryFK))]
        public int Category { get; set; }

        public Categories CategoryFK { get; set; }

        // relacionamento N-M, com atributos no relacionamento
        public ICollection<Users> Users { get; set; }
    }
}
