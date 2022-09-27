using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class MessageHub: Hub
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _presenceTracker;
        private readonly IUnitOfWork _unitOfWork;

        public MessageHub(IUnitOfWork unitOfWork, 
            IMapper mapper,IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            this._presenceHub = presenceHub;
            this._presenceTracker = presenceTracker;
        }
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUserName();
            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot send messages to yourself");
            }
            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipient == null)  throw new HubException("Recipient not exist.");
            
            var message = new Message()
            {
                Sender = sender,
                SenderUsername = username,
                Recipient = recipient,
                RecipientUsername = createMessageDto.RecipientUsername,
                Content = createMessageDto.Content,
            };
            var groupName = GetGroupName(username, createMessageDto.RecipientUsername);
            var messageGroup = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if (messageGroup != null && messageGroup.Connections.Any(rel=>rel.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _presenceTracker.GetConnectionsForUser(recipient.UserName);
                if(connections != null && connections.Any())
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
                        new {username = sender.UserName, knownAs = sender.KnownAs});
                } 
               
            }
            _unitOfWork.MessageRepository.AddMessage(message);
            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var requestUser = httpContext.User.GetUserName();
            var groupName = GetGroupName(requestUser, otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup",group);

            var messages = await _unitOfWork.MessageRepository.GetMessageThreadAsync(requestUser, otherUser);
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages); 
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup",group);
            await base.OnDisconnectedAsync(exception);
        }
        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if(group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());
            group.Connections.Add(connection);
            if(await _unitOfWork.Complete())
            {
                return group;
            }
            throw new HubException("Failed to join group");
        }
        private async Task<Group> RemoveFromMessageGroup()
        {
            var connection = await _unitOfWork.MessageRepository.GetConnection(Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);
            if(await _unitOfWork.Complete())
            {
                var group = await _unitOfWork.MessageRepository.GetMessageGroup(connection.GroupName);
                return group;
            }
            throw new HubException("Failed to remove connection");

        }
        private static string GetGroupName(string requestUser, string otherUser)
        {
            return requestUser.CompareTo(otherUser) > 0 ? $"{otherUser}-{requestUser}" : $"{requestUser}-{otherUser}";
        }
    }
}
