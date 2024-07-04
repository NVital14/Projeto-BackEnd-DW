using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Projeto.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Projeto.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Utilizadores> Utilizadores { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<Favorites> Favorites { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Comments> Comments { get; set; }

        public List<int> GetReviewUsers(int? reviewId, int userId)
        {
            var cnn = (SqlConnection)this.Database.GetDbConnection();
            cnn.Open();

            var query = "SELECT UsersId from ReviewsUtilizadores ur  WHERE ur.ReviewsReviewId = @ReviewId and ur.UsersId != @userId;";
            SqlCommand com = new SqlCommand(query, cnn);
            com.Parameters.AddWithValue("@ReviewId", reviewId);
            com.Parameters.AddWithValue("@UserId", userId);

            List<int> userIds = new List<int>();
            using (SqlDataReader reader = com.ExecuteReader())
            {
                while (reader.Read())
                {
                    userIds.Add((int)reader[0]);
                }
            }

            return userIds;
            //return this.Utilizadores.FromSqlRaw(query, new SqlParameter("@ReviewId", reviewId)).ToList();
        }

        public void DeleteColaborator(int reviewId, int userId)
        {
            var cnn = (SqlConnection)this.Database.GetDbConnection();
            //cnn.Open();

            var query = "Delete from ReviewsUtilizadores WHERE ReviewsReviewId = @ReviewId and UsersId = @userId;";
            SqlCommand com = new SqlCommand(query, cnn);
            com.Parameters.AddWithValue("@ReviewId", reviewId);
            com.Parameters.AddWithValue("@UserId", userId);
            com.ExecuteNonQuery();

            //return this.Utilizadores.FromSqlRaw(query, new SqlParameter("@ReviewId", reviewId)).ToList();
        }

    }
}

