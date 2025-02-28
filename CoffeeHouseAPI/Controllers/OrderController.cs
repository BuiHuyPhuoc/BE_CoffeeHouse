using AutoMapper;
using Azure.Core;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Order;
using CoffeeHouseAPI.DTOs.Topping;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.OrderService;
using CoffeeHouseAPI.Services.ProductService;
using CoffeeHouseAPI.Services.VoucherService;
using CoffeeHouseLib.Models;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System.Net;
using System.Runtime.CompilerServices;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IVoucherService _voucherService;
        readonly IProductService _productService;
        readonly IMapper _mapper;
        private readonly IOrderService _orderService;

        public OrderController(DbcoffeeHouseContext context,IProductService productService, IVoucherService voucherService, IMapper mapper, IOrderService orderService)
        {
            _mapper = mapper;
            _context = context;
            _voucherService = voucherService;
            _productService = productService;
            _orderService = orderService;
        }

        [HttpPost]
        [Route("CreateOrderFromCart")]
        [MasterAuth]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderCartDTO request)
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            if (request == null || request.CartIds.Count <= 0)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Chọn ít nhất 1 sản phẩm trong giỏ hàng cần mua",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            Voucher? voucher = null;

            if (request.VoucherId != null)
            {
                voucher = await _context.Vouchers.Where(x => x.Id == request.VoucherId && x.IsActive == true).FirstOrDefaultAsync();

                if (voucher == null || _voucherService.ValidateVoucher(voucher) == false)
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

                    List<CreateOrderDetailDTO> createOrderDetailDTOs = new List<CreateOrderDetailDTO>();

                    foreach (var cartOrderId in request.CartIds)
                    {
                        var cart = _context.Carts
                            .Include(x => x.CartDetails).ThenInclude(y => y.Topping)
                            .Where(x => x.Id == cartOrderId).FirstOrDefault();

                        if (cart == null) throw new BaseException("Mã sản phẩm trong giỏ hàng không tồn tại.");

                        CreateOrderDetailDTO createOrderDetailDTO = new CreateOrderDetailDTO();
                        createOrderDetailDTO.ProductSizeId = cart.ProductSizeId;
                        createOrderDetailDTO.Quantity = cart.Quantity;

                        List<ToppingOrderDTO> toppingOrderDTOs = new List<ToppingOrderDTO>();
                        if (cart.CartDetails.Count > 0)
                        {
                            foreach (var cartDetail in cart.CartDetails)
                            {
                                if (cartDetail.Topping == null)
                                {
                                    throw new BaseException($"Topping không tồn tại.");
                                }
                                else if (cartDetail.Topping.IsValid == false)
                                {
                                    throw new BaseException($"Topping {cartDetail.Topping.ToppingName} đã ngưng bán.");
                                }

                                ToppingOrderDTO toppingOrder = new ToppingOrderDTO();
                                toppingOrder.Id = cartDetail.Topping.Id;
                                toppingOrder.Quantity = cartDetail.Quantity ?? 1;
                                toppingOrderDTOs.Add(toppingOrder);
                            }
                        }

                        createOrderDetailDTO.Toppings = toppingOrderDTOs;
                        createOrderDetailDTO.Note = string.Empty;

                        createOrderDetailDTOs.Add(createOrderDetailDTO);
                    }

                    await CreateOrderDetail(newOrder.Id, createOrderDetailDTOs);

                    List<Cart> deleteCart = new List<Cart>();
                    foreach(var cartOrderId in request.CartIds)
                    {
                        var cart = _context.Carts.Where(x => x.Id == cartOrderId).FirstOrDefault();
                        if (cart != null) deleteCart.Add(cart);
                    }

                    _context.RemoveRange(deleteCart);
                    await this.SaveChanges(_context);

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

        [HttpPost]
        [Route("CreateOrder")]
        [MasterAuth]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO request)
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            Voucher? voucher = null;

            if (request.VoucherId != null)
            {
                voucher = await _context.Vouchers.Where(x => x.Id == request.VoucherId && x.IsActive == true).FirstOrDefaultAsync();

                if (voucher == null || _voucherService.ValidateVoucher(voucher) == false)
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

        [HttpGet]
        [Route(nameof(GetOrder))]
        [MasterAuth]
        public IActionResult GetOrder()
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();
            var orders = _orderService.GetAllOrderWithRelate(true).Where(x => x.CustomerId == loginResponse.Id).ToList();

            var orderDTOs = _mapper.Map<List<OrderResponseDTO>>(orders);
            return Ok(new APIResponseBase
            {
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderDTOs,
                Status = (int)StatusCodes.Status200OK,
                IsSuccess = true
            });
        }

        [HttpGet]
        [Route(nameof(GetAllOrder))]
        [MasterAuth(true)]
        public IActionResult GetAllOrder()
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();
            var orders = _orderService.GetAllOrderWithRelate(true);

            var orderDTOs = _mapper.Map<List<OrderResponseDTO>>(orders);
            return Ok(new APIResponseBase
            {
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderDTOs,
                Status = (int)StatusCodes.Status200OK,
                IsSuccess = true
            });
        }

        [HttpGet]
        [Route(nameof(GetOrderDetail))]
        public IActionResult GetOrderDetail(int orderId)
        {
            var order = _orderService.GetAllOrderWithRelate(true).Where(x => x.Id == orderId).FirstOrDefault();

            if (order == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng không tồn tại",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            var orderDTO = _mapper.Map<OrderResponseDTO>(order);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderDTO
            });
        }

        [HttpPost]
        [Route(nameof(UpdateOrderStatus))]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string statusCode)
        {
            var order = _orderService.GetAllOrderWithRelate(true).Where(x => x.Id == orderId).FirstOrDefault();

            if (order == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng không tồn tại",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            var currentStatus = order.OrderLogs.OrderByDescending(x => x.TimeLog).FirstOrDefault();

            if (currentStatus == null) {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng không có trạng thái",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            if (currentStatus != null && currentStatus.StatusCode == statusCode)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng đã ở trạng thái này",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            if (currentStatus != null && currentStatus.StatusCode == ORDER_STATUS.CANCELED.ToString())
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng đã bị hủy",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            var orderLog = _context.OrderStatuses.FirstOrDefault(x => x.StatusCode == statusCode);

            var listOrderStatus = _context.OrderStatuses.OrderBy(x => x.Index).AsNoTracking().ToList();

            if (orderLog == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Trạng thái đơn hàng không tồn tại",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            List<OrderLog> newOrderLogs = new List<OrderLog>();
            
            for (int i = currentStatus!.StatusCodeNavigation.Index + 1; i <= orderLog.Index; i++)
            {
                OrderLog newOrderLog = new OrderLog();
                newOrderLog.StatusCode = listOrderStatus[i].StatusCode;
                newOrderLog.OrderId = orderId;
                newOrderLog.TimeLog = DateTime.Now;
                newOrderLogs.Add(newOrderLog);
            }

            _context.OrderLogs.AddRange(newOrderLogs);
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
            });
        }

        [HttpPost]
        [Route(nameof(CancelOrder))]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = _orderService.GetAllOrderWithRelate(true).Where(x => x.Id == orderId).FirstOrDefault();

            if (order == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng không tồn tại",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            var currentStatus = order.OrderLogs.OrderByDescending(x => x.TimeLog).FirstOrDefault();

            if (currentStatus == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng không có trạng thái",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            if (currentStatus.StatusCode == ORDER_STATUS.CANCELED.ToString())
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng đã bị hủy",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            if (currentStatus.StatusCode == ORDER_STATUS.DELIVERED.ToString())
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng đã được giao",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            await CreateOrderStatus(orderId, ORDER_STATUS.CANCELED.ToString());

            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
            });
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
                    .Include(x => x.Product).ThenInclude(y => y.Discount)
                    .FirstOrDefault(x => x.Id == orderDetail.ProductSizeId && x.IsValid);

                if (productSize == null)
                    throw new BaseException("Sản phẩm không tồn tại");

                var discount = productSize.Product.Discount;
                OrderDetail newOrderDetail = new OrderDetail
                {
                    OrderId = orderId,
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
