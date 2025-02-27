using AutoMapper;
using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.OrderService
{
    public class OrderService
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;

        public OrderService(DbcoffeeHouseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Order
    }
}
