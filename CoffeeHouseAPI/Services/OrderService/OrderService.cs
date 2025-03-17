using AutoMapper;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Services.ProductService;
using CoffeeHouseAPI.Services.VoucherService;
using CoffeeHouseLib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Services.OrderService
{
    public class OrderService : IOrderService
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;
        readonly IVoucherService _voucherService;
        readonly IProductService _productService;

        public OrderService(DbcoffeeHouseContext context, IMapper mapper, IVoucherService voucherService, IProductService productService)
        {
            _context = context;
            _mapper = mapper;
            _voucherService = voucherService;
            _productService = productService;
        }

        public List<Order> GetAllOrderWithRelate(bool isNoTracking = false)
        {
            IQueryable<Order> orders = _context.Orders
                .Include(x => x.Address)
                .Include(x => x.Customer)
                .Include(x => x.OrderDetails).ThenInclude(y => y.ProductSize).ThenInclude(z => z.Product).ThenInclude(t => t.ImageDefaultNavigation)
                .Include(x => x.OrderDetails).ThenInclude(y => y.ProductSize).ThenInclude(z => z.Product).ThenInclude(t => t.Category)
                .Include(x => x.OrderDetails).ThenInclude(y => y.ProductSize).ThenInclude(z => z.Product).ThenInclude(t => t.Discount)
                .Include(x => x.OrderDetails).ThenInclude(y => y.OrderToppings).ThenInclude(z => z.Topping)
                .Include(x => x.OrderLogs.OrderBy(y => y.TimeLog)).ThenInclude(y => y.StatusCodeNavigation)
                .Include(x => x.Voucher);
            if (orders == null)
            {
                return new List<Order>();
            }

            if (isNoTracking)
            {
                orders = orders.AsNoTracking();
            }

            return orders.ToList();
        }

        public List<Order> GetAllSuccessOrderWithRelate(bool isNoTracking = false)
        {
            IQueryable<Order> orders = _context.Orders
                .Include(x => x.Address)
                .Include(x => x.Customer)
                .Include(x => x.OrderDetails).ThenInclude(y => y.ProductSize).ThenInclude(z => z.Product).ThenInclude(t => t.ImageDefaultNavigation)
                .Include(x => x.OrderDetails).ThenInclude(y => y.ProductSize).ThenInclude(z => z.Product).ThenInclude(t => t.Category)
                .Include(x => x.OrderDetails).ThenInclude(y => y.ProductSize).ThenInclude(z => z.Product).ThenInclude(t => t.Discount)
                .Include(x => x.OrderDetails).ThenInclude(y => y.OrderToppings).ThenInclude(z => z.Topping)
                .Include(x => x.OrderLogs.OrderBy(y => y.TimeLog)).ThenInclude(y => y.StatusCodeNavigation)
                .Include(x => x.Voucher);
            if (orders == null)
            {
                return new List<Order>();
            }

            if (isNoTracking)
            {
                orders = orders.AsNoTracking();
            }

            return orders.Where(x => x.OrderLogs.OrderBy(y => y.TimeLog).LastOrDefault()!.StatusCode == "DELIVERED").ToList();
        }

        public decimal GetTotalPriceAllOrder()
        {
            decimal AllOrderPrice = 0m;

            List<Order> orders = GetAllOrderWithRelate().Where(x => x.OrderLogs.LastOrDefault()!.StatusCode == "DELIVERED").ToList();

            foreach (var order in orders)
            {
                AllOrderPrice += GetTotalPriceOrder(order);
            }

            return AllOrderPrice;
        }

        public decimal GetTotalPriceOrder(Order order)
        {
            decimal orderPrice = 0m;
            
            foreach (var orderDetail in order.OrderDetails)
            {
                decimal productPrice = _productService.GetProductPrice(orderDetail.ProductSize.Product, orderDetail.ProductSize);
                decimal toppingPrice = 0m;
                foreach (var orderTopping in orderDetail.OrderToppings)
                {
                    toppingPrice += orderTopping.Topping.ToppingPrice;
                }
                decimal totalPrice = (productPrice + toppingPrice) * orderDetail.Quantity;
                orderPrice += totalPrice;
            }

            if (order.Voucher != null)
            {
                if (_voucherService.ValidateVoucher(order.Voucher))
                {
                    if (order.Voucher.DiscountType == DiscountType.PERCENT.ToString())
                    {
                        orderPrice -= orderPrice * order.Voucher.DiscountValue / 100;
                    }
                    else
                    {
                        orderPrice -= order.Voucher.DiscountValue;
                    }
                }
            }

            return orderPrice;
        }

        public decimal GetTotalPriceOrder(List<Order> orders)
        {
            decimal AllOrderPrice = 0m;
            foreach (var order in orders)
            {
                AllOrderPrice += GetTotalPriceOrder(order);
            }
            return AllOrderPrice;
        }
    }
}
