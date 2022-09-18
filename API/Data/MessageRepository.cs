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
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.Include(rel => rel.Sender).Include(rel=>rel.Recipient).SingleOrDefaultAsync(msg => msg.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(message => message.MessageSent).AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(msg => msg.Recipient.UserName == messageParams.Username && !msg.RecipientDeleted),
                "Outbox" => query.Where(msg => msg.Sender.UserName == messageParams.Username && !msg.SenderDeleted),
                _ => query.Where(msg => msg.Recipient.UserName == messageParams.Username && msg.DateRead == null && !msg.RecipientDeleted)
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
        {
            var messages = await context.Messages
                .Include(msg => msg.Recipient).ThenInclude(p => p.Photos)
                .Include(msg => msg.Sender).ThenInclude(p=>p.Photos)
                .Where(msg => (msg.Recipient.UserName == recipientUsername 
                            && msg.Sender.UserName == currentUsername && !msg.SenderDeleted) 
                            || (msg.Recipient.UserName == currentUsername 
                            && msg.Sender.UserName == recipientUsername && !msg.RecipientDeleted)
                 )
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            var unreadMessages = messages.Where(msg => msg.DateRead == null 
                                && msg.Recipient.UserName == currentUsername).ToList();

            if(unreadMessages.Any())
            {
                foreach (var unreadMsg in unreadMessages)
                {
                    unreadMsg.DateRead = DateTime.Now;
                }
                await context.SaveChangesAsync();
            }

            return mapper.Map<IList<MessageDto>>(messages);
            
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
