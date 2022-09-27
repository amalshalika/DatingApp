using API.Interfaces;
using AutoMapper;
using System.Threading.Tasks;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UnitOfWork(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public ILikesRepository LikesRepository => new LikesRepository(context);

        public IMessageRepository MessageRepository => new MessageRepository(context, mapper);

        public IUserRepository UserRepository => new UserRepository(context,mapper);

        public async Task<bool> Complete()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return context.ChangeTracker.HasChanges();
        }
    }
}
