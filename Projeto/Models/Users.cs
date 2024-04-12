using System.ComponentModel.DataAnnotations;

namespace Projeto.Models
{
    public class Users
    {
        public Users()
        {
            Reviews = new HashSet<Reviews>();
        }
        [Key]
        public int UserId { get; set; }
        public string Email { get; set; }

        public string Password { get; set; }
        public string UserName { get; set; }

        // relacionamento N-M, com atributos no relacionamento
        public ICollection<Reviews> Reviews { get; set; }
    }
}
