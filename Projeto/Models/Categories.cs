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

        /// <summary>
        /// Id da categoria
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="Id da Categoria")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Nome da categoria
        /// </summary>
        ///  
        [StringLength(20)]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [Display(Name="Nome")]
        public String Name { get; set; }

        // relacionamento 1-N


        /// <summary>
        /// Lista de reviews de cada categoria
        /// </summary>
        public ICollection<Reviews> Reviews { get; set; }


    }
}
