using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Projeto.Models
{
    public class Lists
    {

        /// <summary>
        /// Id da Lista
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ListId { get; set; }

        /// <summary>
        /// Nome da lista
        /// </summary>
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [Display(Name = "Nome")]
        public String ListName { get; set; }


        //relacionamento 1-N
        [ForeignKey(nameof(Utilizador))]
        public int UtilizadorFK { get; set; }

        /// <summary>
        /// Id do user que fez o comentário
        /// </summary>
        public Utilizadores Utilizador { get; set; }

      //  public static implicit operator Lists(Lists v)
        //{
          //  throw new NotImplementedException();
        //}
    }
}
