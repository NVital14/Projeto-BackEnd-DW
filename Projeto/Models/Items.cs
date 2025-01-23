using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Items
    {
        /// <summary>
        /// Id do item
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        /// <summary>
        /// Nome da lista
        /// </summary>
        public String? ItemName { get; set; }

        /// <summary>
        /// Imagem do objeto alvo do item
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Imagem")]
        public String? Image { get; set; } //? vai tornar o campo de preenchimento facultativo

        /// <summary>
        /// Se o item está check ou não
        /// </summary>
        public Boolean IsChecked { get; set; }

        /// <summary>
        /// Preço do item
        /// </summary>
        [Column(TypeName = "decimal(7, 2)")]
        public decimal? Price { get; set; }

        /// <summary>
        /// Quantidade do item
        /// </summary>
        public int Amount { get; set; }

        //relacionamento 1-N
        [ForeignKey(nameof(List))]
        public int ListFK { get; set; }

        /// <summary>
        /// Id do user que fez o comentário
        /// </summary>
        public Lists List { get; set; }
    }
}
