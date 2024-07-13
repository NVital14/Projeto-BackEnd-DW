using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    [PrimaryKey(nameof(UtilizadorFK), nameof(ReviewFK))]
    public class Favorites
    {
        //relacionamento 1-N
        [ForeignKey(nameof(Utilizador))]
        public int UtilizadorFK { get; set; }
        public Utilizadores Utilizador { get; set; }

        [ForeignKey(nameof(Review))]
        public int ReviewFK { get; set; }
        public Reviews Review { get; set; }

        
    }
}
