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

        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }


        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await context.Connections.FindAsync(connectionId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.Include(rel => rel.Sender).Include(rel => rel.Recipient).SingleOrDefaultAsync(msg => msg.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await context.Groups.Include(rel => rel.Connections).FirstOrDefaultAsync(rel => rel.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(message => message.MessageSent)
                        .ProjectTo<MessageDto>(mapper.ConfigurationProvider);
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(msg => msg.RecipientUsername == messageParams.Username && !msg.RecipientDeleted),
                "Outbox" => query.Where(msg => msg.SenderUsername == messageParams.Username && !msg.SenderDeleted),
                _ => query.Where(msg => msg.RecipientUsername == messageParams.Username && msg.DateRead == null && !msg.RecipientDeleted)
            };

            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
        {
            var messages = await context.Messages
                .Include(msg => msg.Recipient).ThenInclude(p => p.Photos)
                .Include(msg => msg.Sender).ThenInclude(p => p.Photos)
                .Where(msg => (msg.Recipient.UserName == recipientUsername
                            && msg.Sender.UserName == currentUsername && !msg.SenderDeleted)
                            || (msg.Recipient.UserName == currentUsername
                            && msg.Sender.UserName == recipientUsername && !msg.RecipientDeleted)
                 )
                .OrderBy(m => m.MessageSent)
                .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
                .ToListAsync();

            var unreadMessages = messages.Where(msg => msg.DateRead == null
                                && msg.RecipientUsername == currentUsername).ToList();

            if (unreadMessages.Any())
            {
                foreach (var unreadMsg in unreadMessages)
                {
                    unreadMsg.DateRead = DateTime.UtcNow;
                }
                await context.SaveChangesAsync();
            }

            return messages;

        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }
    }
}
