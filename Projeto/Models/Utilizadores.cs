using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    public class Utilizadores
    {
        public Utilizadores()
        {
            Reviews = new HashSet<Reviews>();
            Favorites = new HashSet<Favorites>();
            Coments = new HashSet<Coments>();

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }

        // relacionamento 1-N


        /// <summary>
        /// lista dos favoritos do utilizador
        /// </summary>
        public ICollection<Favorites> Favorites { get; set; }

        /// <summary>
        /// lista de comentarios do utilizador
        /// </summary>
        public ICollection<Coments> Coments { get; set; }


        // relacionamento N-M, com atributos no relacionamento
        public ICollection<Reviews> Reviews { get; set; }
    }
}
