using CoffeeHouseAPI.DTOs.Admin;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Order;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.OrderService;
using CoffeeHouseAPI.Services.ProductService;
using CoffeeHouseAPI.Services.VoucherService;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MasterAuth(true)]
    public class AdminController : ControllerBase
    {
        readonly IOrderService _orderService;
        readonly IVoucherService _voucherService;
        readonly IProductService _productService;
        readonly DbcoffeeHouseContext _context;

        public AdminController(IOrderService orderService, IVoucherService voucherService, IProductService productService, DbcoffeeHouseContext context)
        {
            _orderService = orderService;
            _voucherService = voucherService;
            _productService = productService;
            _context = context;
        }

        [HttpGet]
        [Route(nameof(GetAdminHomePage))]
        public IActionResult GetAdminHomePage()
        {
            var getTotalOrder = _orderService.GetAllOrderWithRelate(true);

            var adminHomePageDTO = new AdminHomePageDTO
            {
                TotalUsers = _context.Customers.Where(x => x.IsValid).AsNoTracking().Count(),
                TotalProducts = _productService.GetProductWithRelate(true).Count(),
                TotalOrders = getTotalOrder.Count,
                TotalCompletedOrders = getTotalOrder.Where(x => x.OrderLogs.LastOrDefault()!.StatusCode == "DELIVERED").Count(),
                TotalPendingOrders = getTotalOrder.Where(x => x.OrderLogs.LastOrDefault()!.StatusCode != "DELIVERED" && x.OrderLogs.LastOrDefault()!.StatusCode != "CANCELED").Count(),
                TotalCancelledOrders = getTotalOrder.Where(x => x.OrderLogs.LastOrDefault()!.StatusCode == "CANCELED").Count(),
                TotalRevenue = _orderService.GetTotalPriceAllOrder(),
                CurrentVoucherCount = _voucherService.GetAllVoucherWithRalate(true).Count()
            };

            return Ok(new APIResponseBase
            {
                Status = StatusCodes.Status200OK,
                Message = "Get Admin Home Page Success",
                Value= adminHomePageDTO,
                IsSuccess = true
            });
        }

        [HttpGet]
        [Route(nameof(GetRevenueByTime))]
        public IActionResult GetRevenueByTime([FromQuery] DateTime FromDate, [FromQuery] DateTime ToDate)
        {
            var getOrderByTime = _orderService.GetAllSuccessOrderWithRelate(true).Where(x => x.OrderDate >= FromDate && x.OrderDate <= ToDate).ToList();
            
            decimal revenue = _orderService.GetTotalPriceOrder(getOrderByTime);

            var orderByTimeDTO = new OrderByTimeDTO
            {
                FromDate = FromDate,
                ToDate = ToDate,
                Revenue = revenue,
                OrderCount = getOrderByTime.Count
            };

            return Ok(new APIResponseBase
            {
                Status = StatusCodes.Status200OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderByTimeDTO,
                IsSuccess = true
            });
        }
    }
}
