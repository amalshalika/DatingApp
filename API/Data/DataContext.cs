using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserLike>()
                .HasKey(k => new { k.SourceUserId, k.LikedUserId });

            builder.Entity<UserLike>()
                .HasOne(user => user.SourceUser)
                .WithMany(user => user.LikedUsers)
                .HasForeignKey(user => user.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(user => user.LikedUser)
                .WithMany(user => user.LikedByUsers)
                .HasForeignKey(user => user.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
