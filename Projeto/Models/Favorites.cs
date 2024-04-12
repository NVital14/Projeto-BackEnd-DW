using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Models
{
    [PrimaryKey(nameof(UserId), nameof(ReviewId))]
    public class Favorites
    {
        //relacionamento 1-N
        [ForeignKey(nameof(UserIdFK))]
        public int UserId { get; set; }

        public Users UserIdFK { get; set; }

        [ForeignKey(nameof(ReviewIdFK))]
        public int ReviewId { get; set; }

        public Reviews ReviewIdFK { get; set; }
    }
}
