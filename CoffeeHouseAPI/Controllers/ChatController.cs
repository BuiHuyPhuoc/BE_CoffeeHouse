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
using System.Runtime.InteropServices;

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
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            var chatRoom = _context.ChatRooms.Where(x => x.Id == request.RoomId).FirstOrDefault();

            bool checkUserIsInRoom = _context.RoomUsers.Any(x => x.RoomId == request.RoomId && x.UserId == loginResponse.Id);

            if (!checkUserIsInRoom)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "User is not in this room",
                    Status = StatusCodes.Status400BadRequest,
                });
            }

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
            message.SenderId = loginResponse.Id;
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
                SenderId = loginResponse.Id,
                Content = request.Content,
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
        [Route(nameof(GetMessageInRoom))]
        public async Task<IActionResult> GetMessageInRoom([FromBody] GetMessageInRoomDTO request)
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();
            var chatRoom = _context.ChatRooms.Where(x => x.Id == request.RoomId && x.IsValid).FirstOrDefault();

            if (chatRoom == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Chat room not found",
                    Status = StatusCodes.Status404NotFound,
                });
            }

            bool checkUserIsInRoom = _context.RoomUsers.Any(x => x.RoomId == request.RoomId && x.UserId == loginResponse.Id);

            if (!checkUserIsInRoom)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "User is not in this room",
                    Status = StatusCodes.Status400BadRequest,
                });
            }

            var messages = await _context.Messages
                .Where(x => x.RoomId == request.RoomId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(request.Quantity)
                .ToListAsync();

            var messageDTOs = _mapper.Map<List<MessageResponseDTO>>(messages);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Get message in room successfully",
                Status = StatusCodes.Status200OK,
                Value = messageDTOs,
            });
        }

        [HttpGet]
        [Route(nameof(GetChatRoom))]
        public async Task<IActionResult> GetChatRoom()
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            var chatRooms = await _context.RoomUsers
                .Where(x => x.UserId == loginResponse.Id && x.IsValid && x.Room.IsValid)
                .Select(x => x.Room)
                .AsNoTracking()
                .ToListAsync();

            var chatRoomDTOs = _mapper.Map<List<ChatRoomResponseDTO>>(chatRooms);
            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = "Get chat room successfully",
                Status = StatusCodes.Status200OK,
                Value = chatRoomDTOs,
            });
        }

        [HttpPost]
        [Route(nameof(CreateGroupChat))]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatDTO request)
        {
            if (request.RoomType != "GROUP" && request.RoomType != "PRIVATE")
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Room type is GROUP or PRIVATE",
                    Status = StatusCodes.Status400BadRequest,
                });
            }

            string groupName = "";
            List<Customer> customers = new List<Customer>();
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            for (int i = 0; i < request.CustomerIds.Count; i++)
            {
                var user = _context.Customers
                    .Where(x => x.Id == request.CustomerIds[i] && x.IsValid && x.Account != null)
                    .FirstOrDefault();
                if (user == null)
                {
                    return Ok(new APIResponseBase
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        Status = StatusCodes.Status404NotFound,
                    });
                }

                if (request.RoomType == "GROUP")
                {
                    if (i == request.CustomerIds.Count - 1)
                    {
                        groupName += user.FullName;
                    }
                    else
                    {
                        groupName += user.FullName + ", ";
                    }
                }

                customers.Add(user);
            }

            ChatRoom newChatRoom = new ChatRoom();
            newChatRoom.Name = groupName;
            newChatRoom.RoomType = request.RoomType;
            newChatRoom.CreatedAt = DateTime.UtcNow;
            newChatRoom.IsValid = true;
            newChatRoom.RoomImage = "https://firebasestorage.googleapis.com/v0/b/shopoes-2e0b8.appspot.com/o/coffee_house%2Fchat_icon.png?alt=media&token=24cadc00-86bf-4d8a-883e-7e81e37798f3";

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
