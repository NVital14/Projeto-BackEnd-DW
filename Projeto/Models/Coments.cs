using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Coments
    {
        [Key]
        public int ComentId { get; set; }

        public String Coment { get; set; }

        //relacionamento 1-N
        [ForeignKey(nameof(UserIdFK))]
        public int UserId { get; set; }

        public Users UserIdFK { get; set; }

        [ForeignKey(nameof(ReviewIdFK))]
        public int ReviewId { get; set; }

        public Reviews ReviewIdFK { get; set; }

    }
}
