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

        /// <summary>
        /// Id do utilizador na tabela Utilizadores
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nome do Utilizador
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// atributo para funcionar como FK entre a tabela dos Utilizadores
        /// e a tabela da Autenticação
        /// </summary>
        public int UserId { get; set; }

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
