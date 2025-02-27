using AutoMapper;
using CoffeeHouseLib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Services.OrderService
{
    public class OrderService : IOrderService
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;

        public OrderService(DbcoffeeHouseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<Order> GetAllOrderWithRelate(bool isNoTracking = false)
        {
            IQueryable<Order> orders = _context.Orders
                .Include(x => x.Address)
                .Include(x => x.Customer)
                .Include(x => x.OrderDetails)
                .Include(x => x.OrderLogs)
                .Include(x => x.Voucher);
            if (isNoTracking)
            {
                orders = orders.AsNoTracking();
            }

            return orders.ToList();
        }
    }
}
