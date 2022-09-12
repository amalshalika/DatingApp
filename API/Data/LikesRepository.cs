
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext context;

        public LikesRepository(DataContext context)
        {
            this.context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {

            return await context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParam)
        {
            var users = context.Users.OrderBy(user => user.UserName).AsQueryable();
            var likes = context.Likes.AsQueryable();
            if (string.IsNullOrEmpty(likesParam.Predicate) || (likesParam.Predicate.ToLower() != "liked"
                && likesParam.Predicate.ToLower() != "likedby"))
            {
                return await PagedList<LikeDto>.CreateAsync(Enumerable.Empty<LikeDto>().AsQueryable(), 0, 0);
            }

            if (likesParam.Predicate.ToLower() == "liked")
            {
                likes = likes.Where(rel => rel.SourceUserId == likesParam.UserId);
                users = likes.Select(rel => rel.LikedUser);
            }

            if (likesParam.Predicate.ToLower() == "likedby")
            {
                likes = likes.Where(rel => rel.LikedUserId == likesParam.UserId);
                users = likes.Select(rel => rel.SourceUser);
            }

            var likeUsers = users.Select(user => new LikeDto()
            {
                Id = user.Id,
                Age = user.DateOfBirth.CalculateAge(),
                City = user.City,
                KnownAs = user.KnownAs,
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                Username = user.UserName
            });
            return await PagedList<LikeDto>.CreateAsync(likeUsers,
                likesParam.PageNumber, likesParam.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await context.Users.Include(rel => rel.LikedUsers).FirstOrDefaultAsync(user => user.Id == userId);
        }
    }
}
