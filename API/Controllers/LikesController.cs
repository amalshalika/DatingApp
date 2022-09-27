using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Autorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public LikesController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> AddLike(string username)
        {
            var likeUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(User.GetUserId());

            if (likeUser == null) return NotFound($"User with username {username} not exist.");

            if (sourceUser.UserName == username)
            {
                return BadRequest("You cannot like yourself");
            }

            if (await _unitOfWork.LikesRepository.GetUserLike(sourceUser.Id, likeUser.Id) != null)
            {
                return BadRequest("User has already liked");
            }
            var newLikeUser = new UserLike() { SourceUserId = sourceUser.Id, LikedUserId = likeUser.Id };

            sourceUser.LikedUsers.Add(newLikeUser);
            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("Failed to like user");

        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }

    }
}
