using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Services.HubService
{
    public class UserConnectionHub : Hub
    {
        private readonly DbcoffeeHouseContext _context;

        public UserConnectionHub(DbcoffeeHouseContext context)
        {
            _context = context;
        }

        public async Task RegisterConnection(int userId)
        {
            var connectionId = Context.ConnectionId;
            var existingConnection = await _context.UserConnections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ConnectionId == connectionId);
            if (existingConnection != null)
            {
                existingConnection.ConnectedAt = DateTime.UtcNow;
                existingConnection.IsOnline = true;
            }
            else
            {
                _context.UserConnections.Add(new UserConnection
                {
                    UserId = userId,
                    ConnectionId = connectionId,
                    ConnectedAt = DateTime.UtcNow,
                    IsOnline = true
                });
            }

            await _context.SaveChangesAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connection = await _context.UserConnections
                .FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);

            if (connection != null)
            {
                connection.IsOnline = false;
                await _context.SaveChangesAsync();
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
