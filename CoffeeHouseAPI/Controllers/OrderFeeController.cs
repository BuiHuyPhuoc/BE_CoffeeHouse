using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.OrderFee;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderFeeController : Controller
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;

        public OrderFeeController(DbcoffeeHouseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [Route(nameof(GetOrderFee))]
        [MasterAuth]
        public IActionResult GetOrderFee()
        {
            var orderFees = _context.OrderFees
                .Where(x => x.IsActive
                && (x.EndDate == null || x.EndDate > DateTime.UtcNow)
                && (x.StartDate == null || x.StartDate < DateTime.UtcNow))
                .AsNoTracking()
                .ToList();

            var orderFeeDTOs = _mapper.Map<List<OrderFeeResponseDTO>>(orderFees);

            return Ok(new APIResponseBase
            {
                Status = StatusCodes.Status200OK,
                Message = "Get order fee successfully",
                Value = orderFeeDTOs,
                IsSuccess = true
            });
        }
    }
}
