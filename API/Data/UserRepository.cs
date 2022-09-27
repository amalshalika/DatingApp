using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(rel=>rel.Photos).SingleOrDefaultAsync(rel=>rel.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(rel=> rel.Photos).ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
        public async Task<MemberDto> GetMemberByUsernameAsync(string username)
        {
            return await _context.Users.Include(rel => rel.Photos).Where(user => user.UserName == username).
                ProjectTo<MemberDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();
        }
        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.Include(rel => rel.Photos)
                .Where(user => user.UserName != userParams.CurrentUserName && user.Gender == userParams.Gender)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);

            var maxDob = DateTime.Now.AddYears(-userParams.MinAge);
            var minDob = DateTime.Now.AddYears(-userParams.MaxAge -1);
            query = query.Where(user => user.DateOfBirth > minDob && user.DateOfBirth < maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
                _=> query.OrderByDescending(x => x.LastActive)
            };
            return await PagedList<MemberDto>.CreateAsync(query.AsNoTracking(), userParams.PageNumber,userParams.PageSize);

        }

        public async Task<string> GetGenderAsync(string username)
        {
            return await _context.Users
                .Where(user => user.UserName == username)
                .Select(user => user.Gender)
                .FirstOrDefaultAsync();
        }
    }
}
