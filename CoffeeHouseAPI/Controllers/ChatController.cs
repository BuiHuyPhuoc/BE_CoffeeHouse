using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Chat;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MasterAuth]
    public class ChatController : TCHControllerBase
    {
        private readonly IHubContext<MessageHub> _hubContext;
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;

        public ChatController(DbcoffeeHouseContext context, IMapper mapper, IHubContext<MessageHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route(nameof(SendMessage))]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO request)
        {
            var chatRoom = _context.ChatRooms.Where(x => x.Id == request.RoomId).FirstOrDefault();

            if (chatRoom == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Chat room not found",
                    Status = StatusCodes.Status404NotFound,
                });
            }


            var message = _mapper.Map<Message>(request);
            message.Status = "Sent";
            message.CreatedAt = DateTime.UtcNow;
            _context.Messages.Add(message);
            await this.SaveChanges(_context);

            chatRoom.LastMessageAt = DateTime.UtcNow;
            await this.SaveChanges(_context);

            var usersInRoom = await _context.RoomUsers
            .Where(ru => ru.RoomId == request.RoomId)
            .Select(ru => ru.UserId)
            .ToListAsync();

            var connections = await _context.UserConnections
                .Where(uc => usersInRoom.Contains(uc.UserId))
                .Select(uc => uc.ConnectionId)
                .ToListAsync();

            await _hubContext.Clients.Clients(connections).SendAsync("ReceiveMessage", new
            {
                RoomId = request.RoomId,
                SenderId = request.SenderId,
                Content = request.Message,
                CreatedAt = message.CreatedAt
            });

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Message sent successfully",
                Status = StatusCodes.Status200OK,
                Value = message.Id,
            });
        }

        [HttpPost]
        [Route(nameof(CreateGroupChat))]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatDTO request)
        {
            string groupName = "";
            List<Customer> customers = new List<Customer>();
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            for (int i = 0; i < request.CustomerIds.Count; i++)
            {
                var user = _context.Customers.Where(x => x.Id == request.CustomerIds[i] && x.IsValid && x.Account != null).FirstOrDefault();
                if (user == null)
                {
                    return Ok(new APIResponseBase
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        Status = StatusCodes.Status404NotFound,
                    });
                }

                if (i == request.CustomerIds.Count - 1)
                {
                    groupName += user.FullName;
                }
                else
                {
                    groupName += ", " + user.FullName;
                }

                customers.Add(user);
            }

            ChatRoom newChatRoom = new ChatRoom();
            newChatRoom.Name = groupName;
            newChatRoom.RoomType = request.RoomType;
            newChatRoom.CreatedAt = DateTime.UtcNow;

            _context.ChatRooms.Add(newChatRoom);
            await this.SaveChanges(_context);

            List<RoomUser> newUsers = new List<RoomUser>();
            var roomAdmin = _context.Customers
                    .Where(x => x.Id == loginResponse.Id && x.IsValid && x.Account != null)
                    .AsNoTracking()
                    .FirstOrDefault();

            if (roomAdmin == null)
            {
                return Unauthorized();
            }

            RoomUser adminRoomUser = new RoomUser();
            adminRoomUser.UserId = roomAdmin.Id;
            adminRoomUser.RoomId = newChatRoom.Id;
            adminRoomUser.JoinedAt = DateTime.UtcNow;
            adminRoomUser.Role = "Admin";
            adminRoomUser.IsValid = true;
            newUsers.Add(adminRoomUser);

            foreach (var user in customers)
            {
                RoomUser roomUser = new RoomUser();
                roomUser.UserId = user.Id;
                roomUser.RoomId = newChatRoom.Id;
                roomUser.JoinedAt = DateTime.UtcNow;
                roomUser.Role = "member";
                roomUser.IsValid = true;
                newUsers.Add(roomUser);
            }

            _context.RoomUsers.AddRange(newUsers);
            await this.SaveChanges(_context);


            return Ok(new APIResponseBase
            {
                Message = "Group chat created successfully",
                Status = StatusCodes.Status200OK,
                Value = newChatRoom.Id,
            });
        }
    }
}
