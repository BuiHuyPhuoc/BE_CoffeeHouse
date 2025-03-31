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
}
