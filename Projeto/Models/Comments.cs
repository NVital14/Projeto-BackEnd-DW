using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Comments
    {
        /// <summary>
        /// Id do comentário
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }

        /// <summary>
        /// Texto do comentário
        /// </summary>
        [StringLength(300, ErrorMessage = "O {0} não pode exceder 30 caracteres.")]
        [Display(Name ="Comentário")]
        public String Comment { get; set; }

        
        //relacionamento 1-N
        [ForeignKey(nameof(Utilizador))]
        public int UtilizadorFK { get; set; }

        /// <summary>
        /// Id do user que fez o comentário
        /// </summary>
        public Utilizadores Utilizador { get; set; }

        [ForeignKey(nameof(Review))]
        public int ReviewFK { get; set; }

        /// <summary>
        /// Id da review onde foi feito o comentário
        /// </summary>
        public Reviews Review { get; set; }

        

    }
}
