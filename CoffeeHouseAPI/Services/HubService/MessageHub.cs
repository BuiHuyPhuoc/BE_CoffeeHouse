using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class MessageHub : Hub
{
    private readonly DbcoffeeHouseContext _context;

    public MessageHub(DbcoffeeHouseContext context)
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

    public async Task SendMessage(Guid roomId, int senderId, string content)
    {
        var message = new Message
        {
            RoomId = roomId,
            SenderId = senderId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var room = await _context.ChatRooms.FindAsync(roomId);

        if (room != null)
        {
            room.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var userInRoom = await _context.RoomUsers
            .Where(c => c.RoomId == roomId)
            .Select(c => c.UserId)
            .ToListAsync();

        var connections = await _context.UserConnections
            .Where(c => userInRoom.Contains(c.UserId))
            .Select(c => c.ConnectionId)
            .ToListAsync();

        await Clients.Clients(connections).SendAsync("ReceiveMessage", new
        {
            RoomId = roomId,
            SenderId = senderId,
            Content = content,
            CreatedAt = message.CreatedAt
        });
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

    public async Task SendMessage(int userId, string message)
    {
        var connectionIds = await _context.UserConnections
            .Where(c => c.UserId == userId)
            .Select(c => c.ConnectionId)
            .ToListAsync();

        foreach (var connectionId in connectionIds)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }
    }
}
