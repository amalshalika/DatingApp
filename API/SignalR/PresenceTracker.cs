using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private static Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();
        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isNewUser = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    isNewUser = true;
                    OnlineUsers.Add(username, new List<string>() { connectionId });
                }
            }
            return Task.FromResult(isNewUser);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isUserOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isUserOffline);
                OnlineUsers[username].Remove(connectionId);
                if (OnlineUsers[username].Count == 0)
                {
                    isUserOffline = true;
                    OnlineUsers.Remove(username);
                }
            }
            return Task.FromResult(isUserOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            lock(OnlineUsers)
            {
                return Task.FromResult(OnlineUsers.Keys.Select(rel => rel).OrderBy(rel => rel).ToArray());

            }
        }
        public Task<IList<string>> GetConnectionsForUser(string username)
        {
            IList<string> connections = new List<string>();
            lock(OnlineUsers)
            {
                connections = OnlineUsers.GetValueOrDefault(username);
            }
            return Task.FromResult(connections);
        }
    }
}
