using Azure.Core;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Order;
using CoffeeHouseAPI.DTOs.Topping;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Google.Apis.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System.Net;
using System.Runtime.CompilerServices;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MasterAuth]
    public class OrderController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;

        public OrderController(DbcoffeeHouseContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO request)
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            Voucher? voucher = null;

            if (request.VoucherId != null)
            {
                voucher = await _context.Vouchers.Where(x => x.Id == request.VoucherId && x.IsActive == true).FirstOrDefaultAsync();

                if (voucher == null)
                {
                    return BadRequest(new APIResponseBase
                    {
                        IsSuccess = false,
                        Message = "Voucher không hợp lệ",
                        Status = (int)HttpStatusCode.BadRequest
                    });
                }
            }

            Address? address = _context.Addresses.Where(x => x.Id == request.AddressId && x.CustomerId == loginResponse.Id).FirstOrDefault();

            if (address == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Địa chỉ không hợp lệ",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }


            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    Order newOrder = new Order();
                    newOrder.VoucherId = voucher?.Id;
                    newOrder.CustomerId = loginResponse.Id;
                    newOrder.OrderDate = DateTime.Now;
                    newOrder.AddressId = address.Id;

                    _context.Orders.Add(newOrder);
                    await this.SaveChanges(_context);

                    await CreateOrderStatus(newOrder.Id, ORDER_STATUS.BOOKED.ToString());

                    await CreateOrderDetail(newOrder.Id, request.OrderDetails);

                    await transaction.CommitAsync();
                    return Ok(new APIResponseBase
                    {
                        IsSuccess = true,
                        Status = (int)HttpStatusCode.OK,
                        Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
                    });
                }
                catch (BaseException baseEx)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new APIResponseBase
                    {
                        IsSuccess = false,
                        Message = baseEx.Message,
                        Status = (int)HttpStatusCode.BadRequest,
                    });
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(e.Message);
                }
            }
        }

        private async Task CreateOrderStatus(int id, string v)
        {
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = id;
            orderLog.StatusCode = v;
            orderLog.TimeLog = DateTime.Now;

            await _context.OrderLogs.AddAsync(orderLog);
        }

        private async Task CreateOrderDetail(int orderId, List<CreateOrderDetailDTO> orderDetails)
        {
            foreach (var orderDetail in orderDetails)
            {
                var productSize = _context.ProductSizes
                    .Include(x => x.Product).ThenInclude(y => y.ProductDiscounts)
                    .FirstOrDefault(x => x.Id == orderDetail.ProductSizeId && x.IsValid);

                if (productSize == null)
                    throw new BaseException("Sản phẩm không tồn tại");

                var discount = productSize.Product.ProductDiscounts.FirstOrDefault(x => x.IsActive);
                OrderDetail newOrderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    DiscountId = discount?.DiscountId,
                    ProductSizeId = productSize.Id,
                    Note = orderDetail.Note,
                    Quantity = orderDetail.Quantity
                };

                _context.OrderDetails.Add(newOrderDetail);
                await this.SaveChanges(_context);

                if (orderDetail.Toppings != null)
                {
                    await CreateOrderTopping(newOrderDetail.Id, orderDetail.Toppings);
                }
            }
        }

        private async Task CreateOrderTopping(int id, List<ToppingOrderDTO> toppings)
        {
            foreach (var orderTopping in toppings)
            {
                var topping = _context.Toppings.FirstOrDefault(x => x.IsValid && x.Id == orderTopping.Id);

                if (topping == null)
                    throw new BaseException("Topping không tồn tại");

                OrderTopping newOrderTopping = new OrderTopping
                {
                    ToppingId = orderTopping.Id,
                    Quantity = orderTopping.Quantity,
                    OrderDetailId = id
                };

                _context.OrderToppings.Add(newOrderTopping);
                await this.SaveChanges(_context);
            }
        }
    }
}
