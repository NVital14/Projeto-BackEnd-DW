using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Projeto.Models;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Projeto.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            /* Esta instrução importa tudo o que está pre-definido
             * na super classe
             */
            base.OnModelCreating(builder);
            //criar um role
            builder.Entity<IdentityRole>().HasData(
             new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" }
             );
        }

        public DbSet<Utilizadores> Utilizadores { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<Favorites> Favorites { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Comments> Comments { get; set; }

        public List<int> GetReviewUsers(int reviewId, int userId, bool isJustCollaborators)
        {
            var cnn = (SqlConnection)this.Database.GetDbConnection();
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            var query = "";

            //quero só os colaboradores da review, sem o utilizador atual
            if (isJustCollaborators)
            {
            query = "SELECT UsersId from ReviewsUtilizadores ur  WHERE ur.ReviewsReviewId = @ReviewId and ur.UsersId != @userId;";

            }
            //quero todos os utilizadores associados à review
            else
            {
                query = "SELECT UsersId from ReviewsUtilizadores ur  WHERE ur.ReviewsReviewId = @ReviewId;";
            }
            SqlCommand com = new SqlCommand(query, cnn);
            if (isJustCollaborators)
            {
            com.Parameters.AddWithValue("@UserId", userId);

            }
            com.Parameters.AddWithValue("@ReviewId", reviewId);

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

        public void DeleteCollaborator(int reviewId, int userId)
        {
            var cnn = (SqlConnection)this.Database.GetDbConnection();
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            var query = "Delete from ReviewsUtilizadores WHERE ReviewsReviewId = @ReviewId and UsersId = @userId;";
            SqlCommand com = new SqlCommand(query, cnn);
            com.Parameters.AddWithValue("@ReviewId", reviewId);
            com.Parameters.AddWithValue("@UserId", userId);
            com.ExecuteNonQuery();

            //return this.Utilizadores.FromSqlRaw(query, new SqlParameter("@ReviewId", reviewId)).ToList();
        }

    }
}

