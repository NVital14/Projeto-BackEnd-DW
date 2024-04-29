using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Reviews
    {
        public Reviews() {
            Users = new HashSet<Utilizadores>();
            Coments = new HashSet<Coments>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public String Image { get; set; }

        public int Rating { get; set;}

        public Boolean IsShared { get; set; }

        //relacionamento 1-N
        [ForeignKey(nameof(Category))]

        public Categories CategoryFK { get; set; }
        public int Category { get; set; }

        // relacionamento 1-N

        /// <summary>
        /// lista de comentarios do utilizador
        /// </summary>
        public ICollection<Coments> Coments { get; set; }

        // relacionamento N-M, com atributos no relacionamento
        public ICollection<Utilizadores> Users { get; set; }
    }
}
