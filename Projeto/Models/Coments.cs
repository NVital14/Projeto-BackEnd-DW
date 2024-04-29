using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Coments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComentId { get; set; }

        public String Coment { get; set; }

        //relacionamento 1-N
        [ForeignKey(nameof(UserId))]
        public Utilizadores UserIdFK { get; set; }
        public int UserId { get; set; }

        [ForeignKey(nameof(ReviewId))]
        public Reviews ReviewIdFK { get; set; }
        public int ReviewId { get; set; }

        

    }
}
