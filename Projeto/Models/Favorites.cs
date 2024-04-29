using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    [PrimaryKey(nameof(UserId), nameof(ReviewId))]
    public class Favorites
    {
        //relacionamento 1-N
        [ForeignKey(nameof(UserId))]
        public Utilizadores UserIdFK { get; set; }
        public int UserId { get; set; }

        [ForeignKey(nameof(ReviewId))]
        public Reviews ReviewIdFK { get; set; }
        public int ReviewId { get; set; }

        
    }
}
