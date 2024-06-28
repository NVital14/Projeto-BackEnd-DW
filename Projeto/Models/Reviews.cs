using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Reviews
    {
        public Reviews() {
            Users = new HashSet<Utilizadores>();
            Coments = new HashSet<Coments>();
            Favorites = new HashSet<Favorites>();
        }
        /// <summary>
        /// Id da review
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="Id da Review")]
        public int ReviewId { get; set; }

        /// <summary>
        /// Título da review
        /// </summary>
        [StringLength(50, ErrorMessage = "O {0} não pode exceder 50 caracteres.")]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [Display(Name = "Título")]
        public String Title { get; set; }

        /// <summary>
        /// Descrição da review
        /// </summary>
        [StringLength(3000, ErrorMessage = "O {0} não pode exceder 3000 caracteres.")]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [Display(Name = "Descrição")]
        public String Description { get; set; }

        /// <summary>
        /// Imagem do objeto alvo da review
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Imagem")]
        public String? Image { get; set; } //? vai tornar o campo de preenchimento facultativo

        /// <summary>
        /// Pontuação dada ao objeto alvo da review
        /// </summary>
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório.")]
        [Display(Name = "Pontuação")]
        public int Rating { get; set;}

        /// <summary>
        /// Variável que permite saber se esta review está publicada para todos os utilizadores ou se a review ainda é privada
        /// </summary>
        [Display(Name = "Patilhado")]
        public Boolean IsShared { get; set; }

        //relacionamento 1-N
        [ForeignKey(nameof(Category))]

        [Display(Name ="Categoria")]
        public int CategoryFK { get; set; }
        /// <summary>
        /// Categoria em que a review está inserida
        /// </summary>
        [Display(Name = "Categoria")]
        public Categories? Category { get; set; }

        // relacionamento 1-N

        /// <summary>
        /// lista de comentarios do utilizador
        /// </summary>
        public ICollection<Coments> Coments { get; set; }

        /// <summary>
        /// lista de favoritos 
        /// </summary>
        public ICollection<Favorites> Favorites { get; set; }

        // relacionamento N-M, com atributos no relacionamento
        public ICollection<Utilizadores> Users { get; set; }
    }
}
