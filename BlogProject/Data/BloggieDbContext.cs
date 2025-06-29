using Microsoft.EntityFrameworkCore;
using MyFirstApp.Models.Domain;

namespace MyFirstApp.Data
{
    public class BloggieDbContext : DbContext
    {
        public BloggieDbContext(DbContextOptions<BloggieDbContext> options) : base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<BlogPostLike> BlogPostLike { get; set; }
    }
}
