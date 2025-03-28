using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Order;
using CoffeeHouseAPI.DTOs.Topping;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.HubService;
using CoffeeHouseAPI.Services.OrderService;
using CoffeeHouseAPI.Services.ProductService;
using CoffeeHouseAPI.Services.VoucherService;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Net;

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
        readonly IHubContext<OrderHub> _hubContext;
        private readonly IConfiguration _configuration;

        public OrderController(DbcoffeeHouseContext context,
                                IProductService productService,
                                IVoucherService voucherService,
                                IMapper mapper,
                                IOrderService orderService,
                                IHubContext<OrderHub> hubContext,
                                IConfiguration configuration)
        {
            _mapper = mapper;
            _context = context;
            _voucherService = voucherService;
            _productService = productService;
            _orderService = orderService;
            _hubContext = hubContext;
            _configuration = configuration;
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
            //decimal totalPrice = GetPriceFromRequestCreateOrder(request);

            request.CartIds = request.CartIds.Distinct().ToList();

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
                    newOrder.OrderPaid = 0;
                    newOrder.OrderValue = 0;
                    newOrder.PaymentMethod = request.PaymentMethod;
                    newOrder.PaymentId = newOrder.OrderDate.ToString("yyyyMMddHHmmss");

                    _context.Orders.Add(newOrder);
                    await this.SaveChanges(_context);

                    await CreateOrderStatus(newOrder.Id, ORDER_STATUS.BOOKED.ToString());

                    List<CreateOrderDetailDTO> createOrderDetailDTOs = new List<CreateOrderDetailDTO>();

                    decimal orderValue = 0;

                    foreach (var cartOrderId in request.CartIds)
                    {
                        var cart = _context.Carts
                            .Include(x => x.CartDetails).ThenInclude(y => y.Topping)
                            .Include(x => x.ProductSize).ThenInclude(y => y.Product).ThenInclude(z => z.Discount)
                            .AsNoTracking()
                            .Where(x => x.Id == cartOrderId)
                            .FirstOrDefault();

                        if (cart == null) throw new BaseException("Mã sản phẩm trong giỏ hàng không tồn tại.");

                        CreateOrderDetailDTO createOrderDetailDTO = new CreateOrderDetailDTO();
                        createOrderDetailDTO.ProductSizeId = cart.ProductSizeId;
                        createOrderDetailDTO.Quantity = cart.Quantity;

                        orderValue += _productService.GetProductPrice(cart.ProductSize.Product, cart.ProductSize) * cart.Quantity;

                        decimal toppingValue = 0;

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
                                toppingValue += cartDetail.Topping.ToppingPrice * cartDetail.Quantity ?? 1;
                            }
                        }

                        createOrderDetailDTO.Toppings = toppingOrderDTOs;
                        createOrderDetailDTO.Note = string.Empty;

                        createOrderDetailDTOs.Add(createOrderDetailDTO);

                        orderValue += toppingValue * cart.Quantity;
                    }

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

                        if (voucher.MinOrderValue > orderValue)
                        {
                            return BadRequest(new APIResponseBase
                            {
                                IsSuccess = false,
                                Message = "Tổng giá trị đơn hàng không đủ để sử dụng voucher",
                                Status = (int)HttpStatusCode.BadRequest
                            });
                        }
                        else
                        {
                            orderValue = _voucherService.PriceAfterApplyVoucher(orderValue, voucher);
                            newOrder.VoucherId = voucher.Id;
                        }
                    }

                    newOrder.OrderValue = orderValue;

                    await this.SaveChanges(_context);

                    await CreateOrderDetail(newOrder.Id, createOrderDetailDTOs);

                    List<Cart> deleteCart = new List<Cart>();
                    foreach (var cartOrderId in request.CartIds)
                    {
                        var cart = _context.Carts.Where(x => x.Id == cartOrderId).FirstOrDefault();
                        if (cart != null) deleteCart.Add(cart);
                    }

                    _context.RemoveRange(deleteCart);
                    await this.SaveChanges(_context);

                    await transaction.CommitAsync();

                    await _hubContext.Clients.All.SendAsync("OrderUpdated");

                    string urlPayment = string.Empty;
                    if (request.PaymentMethod == "ONLINE")
                    {
                        urlPayment = CreateUrlPayment(newOrder);
                    }
                    else
                    {
                        urlPayment = request.ReturnUrl ?? "http://192.168.77.101:3000/";
                    }

                    return Ok(new APIResponseBase
                    {
                        IsSuccess = true,
                        Status = (int)HttpStatusCode.OK,
                        Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
                        Value = urlPayment
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

                    await _hubContext.Clients.All.SendAsync("OrderUpdated");

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
            var orders = _orderService.GetAllOrderWithRelate(true)
                        .Where(x => x.CustomerId == loginResponse.Id)
                        .OrderByDescending(x => x.OrderDate)
                        .ToList();

            var orderDTOs = _mapper.Map<List<OrderResponseDTO>>(orders);
            return Ok(new APIResponseBase
            {
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderDTOs,
                Status = (int)StatusCodes.Status200OK,
                IsSuccess = true
            });
        }

        /// <summary>
        /// Last test: 28/02/2025
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(GetAllOrder))]
        [MasterAuth(true)]
        public IActionResult GetAllOrder()
        {
            var orders = _orderService.GetAllOrderWithRelate(true).OrderByDescending(x => x.OrderDate);

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
        [Route(nameof(GetAllCustomerOrder))]
        [MasterAuth()]
        public IActionResult GetAllCustomerOrder()
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            var orders = _orderService.GetAllOrderWithRelate(true).OrderByDescending(x => x.OrderDate).Where(x => x.CustomerId == loginResponse.Id).ToList();

            var orderDTOs = _mapper.Map<List<OrderResponseDTO>>(orders);
            return Ok(new APIResponseBase
            {
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderDTOs,
                Status = (int)StatusCodes.Status200OK,
                IsSuccess = true
            });
        }

        /// <summary>
        /// Last test: 28/02/2025
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(GetOrderDetail))]
        [MasterAuth(true)]
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

        /// <summary>
        /// Last test: 28/02/2025
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(UpdateOrderStatus))]
        [MasterAuth(true)]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateStatusOrderDTO request)
        {
            var order = _orderService.GetAllOrderWithRelate(true).Where(x => x.Id == request.OrderId).FirstOrDefault();

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

            if (currentStatus != null && currentStatus.StatusCode == request.StatusCode)
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

            var orderLog = _context.OrderStatuses.FirstOrDefault(x => x.StatusCode == request.StatusCode);

            var listOrderStatus = _context.OrderStatuses.Where(x => x.Index > 0).OrderBy(x => x.Index).AsNoTracking().ToList();

            if (orderLog == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Trạng thái đơn hàng không tồn tại",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            if (orderLog.StatusCode == ORDER_STATUS.CANCELED.ToString() && currentStatus!.StatusCode == ORDER_STATUS.DELIVERING.ToString())
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng đang giao, không thể huỷ.",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            List<OrderLog> newOrderLogs = new List<OrderLog>();

            if (orderLog.Index != 0)
            {
                for (int i = currentStatus!.StatusCodeNavigation.Index + 1; i <= orderLog.Index; i++)
                {
                    OrderLog newOrderLog = new OrderLog();
                    newOrderLog.StatusCode = listOrderStatus[i - 1].StatusCode;
                    newOrderLog.OrderId = request.OrderId;
                    newOrderLog.TimeLog = DateTime.Now;
                    newOrderLogs.Add(newOrderLog);
                }
            }
            else
            {
                OrderLog newOrderLog = new OrderLog();
                newOrderLog.StatusCode = orderLog.StatusCode;
                newOrderLog.OrderId = request.OrderId;
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
        [MasterAuth(true)]
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

        [HttpGet]
        [Route(nameof(GetAllOrderStatus))]
        [MasterAuth(true)]
        public IActionResult GetAllOrderStatus()
        {
            var orderStatuses = _context.OrderStatuses.ToList();
            var orderStatusDTOs = _mapper.Map<List<OrderStatusDTO>>(orderStatuses);
            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderStatusDTOs
            });
        }

        [HttpGet]
        [Route(nameof(GetOrderStatus))]
        [MasterAuth]
        public IActionResult GetOrderStatus()
        {
            var orderStatuses = _context.OrderStatuses.Where(x => x.Index > 0).ToList();
            var orderStatusDTOs = _mapper.Map<List<OrderStatusDTO>>(orderStatuses);
            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderStatusDTOs
            });
        }


        [HttpPost]
        [Route("UpdatePaymentStatus")]
        [MasterAuth]
        public async Task<IActionResult> UpdatePaymentStatus([FromBody] UpdatePaymentStatusDTO request)
        {
            var order = _orderService.GetAllOrderWithRelate(true).Where(x => x.PaymentId == request.PaymentId).FirstOrDefault();
            if (order == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng không tồn tại",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            if (order.OrderValue != request.PaymentValue)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Giá trị thanh toán không chính xác",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            if (order.OrderPaid > 0)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng đã được thanh toán",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            if (request.PaymentStatus == "SUCCESS")
            {
                order.OrderPaid = order.OrderValue;
                order.PaymentStatus = request.PaymentStatus;
                order.PaymentDate = DateTime.Now;
                order.PaymentId = request.PaymentId;
                order.PaymentMethod = "ONLINE";
                _context.Orders.Update(order);
                await this.SaveChanges(_context);
            }
            else if (request.PaymentStatus == "FAILED")
            {
                order.PaymentStatus = request.PaymentStatus;
                _context.Orders.Update(order);
                await this.SaveChanges(_context);
            }

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
            });

        }

        public class UpdatePaymentStatusDTO
        {
            public string PaymentId { get; set; } = null!;
            public string PaymentStatus { get; set; } = null!;
            public decimal PaymentValue { get; set; }
            public DateInterval PaymentDate { get; set; }
        }

        //private decimal GetPriceFromRequestCreateOrder(CreateOrderCartDTO request)
        //{
        //    decimal orderValue = 0;

        //    foreach (var cartOrderId in request.CartIds)
        //    {
        //        var cart = _context.Carts
        //            .Include(x => x.CartDetails).ThenInclude(y => y.Topping)
        //            .Include(x => x.ProductSize).ThenInclude(y => y.Product).ThenInclude(z => z.Discount)
        //            .Where(x => x.Id == cartOrderId)
        //            .AsNoTracking()
        //            .FirstOrDefault();

        //        if (cart == null) throw new BaseException("Mã sản phẩm trong giỏ hàng không tồn tại.");

        //        CreateOrderDetailDTO createOrderDetailDTO = new CreateOrderDetailDTO();
        //        createOrderDetailDTO.ProductSizeId = cart.ProductSizeId;
        //        createOrderDetailDTO.Quantity = cart.Quantity;

        //        orderValue += _productService.GetProductPrice(cart.ProductSize.Product, cart.ProductSize) * cart.Quantity;

        //        decimal toppingValue = 0;

        //        List<ToppingOrderDTO> toppingOrderDTOs = new List<ToppingOrderDTO>();
        //        if (cart.CartDetails.Count > 0)
        //        {
        //            foreach (var cartDetail in cart.CartDetails)
        //            {
        //                if (cartDetail.Topping == null)
        //                {
        //                    throw new BaseException($"Topping không tồn tại.");
        //                }
        //                else if (cartDetail.Topping.IsValid == false)
        //                {
        //                    throw new BaseException($"Topping {cartDetail.Topping.ToppingName} đã ngưng bán.");
        //                }

        //                ToppingOrderDTO toppingOrder = new ToppingOrderDTO();
        //                toppingOrder.Id = cartDetail.Topping.Id;
        //                toppingOrder.Quantity = cartDetail.Quantity ?? 1;
        //                toppingOrderDTOs.Add(toppingOrder);
        //                toppingValue += cartDetail.Topping.ToppingPrice * cartDetail.Quantity ?? 1;
        //            }
        //        }

        //        createOrderDetailDTO.Toppings = toppingOrderDTOs;
        //        createOrderDetailDTO.Note = string.Empty;

        //        createOrderDetailDTOs.Add(createOrderDetailDTO);

        //        orderValue += toppingValue * cart.Quantity;
        //    }
        //}

        private decimal GetValueCartOrder(CreateOrderCartDTO createOrderCartDTO)
        {
            decimal value = 0;
            foreach (var cartId in createOrderCartDTO.CartIds)
            {
                var cart = _context.Carts
                    .Include(x => x.ProductSize).ThenInclude(y => y.Product).ThenInclude(z => z.Discount)
                    .Where(x => x.Id == cartId)
                    .AsNoTracking()
                    .FirstOrDefault();

                if (cart != null)
                {
                    value += _productService.GetProductPrice(cart.ProductSize.Product, cart.ProductSize) * cart.Quantity;
                }
            }
            return value;
        }

        private string CreateUrlPayment(Order order)
        {
            string vnp_Url = _configuration["PaymentKey:VNPay:vnp_Url"]?.ToString() ?? string.Empty;
            string vnp_TmnCode = _configuration["PaymentKey:VNPay:vnp_TmnCode"]?.ToString() ?? string.Empty;
            string vnp_HashSecret = _configuration["PaymentKey:VNPay:vnp_HashSecret"]?.ToString() ?? string.Empty;
            string vnp_ReturnUrl = _configuration["PaymentKey:VNPay:vnp_ReturnUrl"]?.ToString() ?? string.Empty;

            decimal orderValue = _orderService.GetTotalPriceOrder(order);

            VNPayLibrary vnpay = new VNPayLibrary();

            vnpay.AddRequestData("vnp_Version", VNPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (orderValue * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            vnpay.AddRequestData("vnp_CreateDate", order.OrderDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(HttpContext));
            vnpay.AddRequestData("vnp_Locale", "vn");

            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + order.Id);
            vnpay.AddRequestData("vnp_OrderType", "other");

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", order.PaymentId ?? order.OrderDate.ToString("yyyyMMddHHmmss"));

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return paymentUrl;
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

        [HttpGet]
        [Route("GetOrderPriceById")]
        public IActionResult GetOrderPriceById(int orderId)
        {
            var order = _context.Orders
                .Include(x => x.OrderDetails).ThenInclude(y => y.ProductSize).ThenInclude(z => z.Product).ThenInclude(t => t.Discount)
                .Include(x => x.OrderDetails).ThenInclude(y => y.OrderToppings)
                .Include(x => x.Voucher)
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == orderId);
            
            if (order == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đơn hàng không tồn tại",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            decimal orderValue = _orderService.GetTotalPriceOrder(order);
            
            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = orderValue
            });
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
