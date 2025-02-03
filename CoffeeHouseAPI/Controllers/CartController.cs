using AutoMapper;
using Azure.Core;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Cart;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.Topping;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Net;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;

        public CartController(DbcoffeeHouseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("AddToCart")]
        [MasterAuth]
        public async Task<IActionResult> AddToCart([FromBody] CartRequestDTO request)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            var getProductSize = _context.ProductSizes.Where(x => x.Id == request.ProductSizeId).FirstOrDefault();
            if (getProductSize == null)
            {
                return BadRequest(new APIResponseBase
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "Product is not existed",
                    IsSuccess = false
                });
            }

            Cart newCart = new Cart();
            newCart.Quantity = request.Quantity;
            newCart.ProductSizeId = getProductSize.Id;
            newCart.CustomerId = loginResponse.Id;
            newCart.CreatedAt = DateTime.Now;
            newCart.UpdatedAt = DateTime.Now;

            await _context.AddAsync(newCart);
            await this.SaveChanges(_context);

            List<CartDetail> newCartDetails = new List<CartDetail>();
            foreach (var subCart in request.Toppings)
            {
                var getTopping = _context.Toppings
                                .Include(x => x.Products)
                                .Where(x => x.IsValid && x.Products.Where(x => x.Id == getProductSize.ProductId).Count() != 0 && x.Id == subCart.Id)
                                .AsNoTracking()
                                .FirstOrDefault();
                if (getTopping == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new APIResponseBase
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Message = "Topping is not valid for this drink.",
                        IsSuccess = false
                    });
                }
                CartDetail detail = new CartDetail();
                detail.CartId = newCart.Id;
                detail.ToppingId = getTopping.Id;
                detail.Quantity = subCart.Quantity;
                newCartDetails.Add(detail);
            }

            if (newCartDetails.Count > 0)
            {
                await _context.CartDetails.AddRangeAsync(newCartDetails);
                await this.SaveChanges(_context);
            }

            await transaction.CommitAsync();
            await transaction.DisposeAsync();

            return Ok(new APIResponseBase
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Add to cart success",
                IsSuccess = true
            });
        }

        [HttpGet]
        [Route("GetCart")]
        [MasterAuth]
        public async Task<IActionResult> GetCart()
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();
            var carts = await _context.Carts
                .Include(x => x.CartDetails).ThenInclude(y => y.Topping)
                .Include(x => x.ProductSize).ThenInclude(y => y.Product).ThenInclude(z => z.Category)
                .Where(x => x.CustomerId == loginResponse.Id)
                .AsNoTracking()
                .ToListAsync();
            List<CartResponseDTO> cartResponseDTOs = new List<CartResponseDTO>();
            foreach (var cart in carts)
            {
                CartResponseDTO cartResponseDTO = new CartResponseDTO();
                cartResponseDTO.CartId = cart.Id;
                cartResponseDTO.ImageDefaultNavigation = _mapper.Map<ImageResponseDTO>(cart.ProductSize.Product.ImageDefaultNavigation);
                cartResponseDTO.ProductId = cart.ProductSize.ProductId;
                cartResponseDTO.ProductName = cart.ProductSize.Product.ProductName;
                cartResponseDTO.ProductSizeName = cart.ProductSize.Size;
                cartResponseDTO.Quantity = cart.Quantity;
                cartResponseDTO.ProductSizeId = cart.ProductSizeId;
                cartResponseDTO.Price = cart.ProductSize.Price;
                cartResponseDTO.CategoryName = cart.ProductSize.Product.Category.CategoryName;
                var cartDetails = cart.CartDetails;
                List<ToppingDTO> toppingDTOs = new List<ToppingDTO>();
                foreach (var topping in cartDetails)
                {
                    toppingDTOs.Add(_mapper.Map<ToppingDTO>(topping.Topping));
                }
                cartResponseDTO.CartDetails = toppingDTOs;
                cartResponseDTOs.Add(cartResponseDTO);
            }
            return Ok(cartResponseDTOs);
        }

        [HttpPost]
        [Route("DeleteCart")]
        [MasterAuth]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();
            var cart = await _context.Carts
                .Include(x => x.CartDetails)
                .Where(x => x.Id == cartId && x.CustomerId == loginResponse.Id)
                .FirstOrDefaultAsync();
            if (cart == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET),
                    Status = (int)HttpStatusCode.BadRequest,
                });
            }
            _context.RemoveRange(cart.CartDetails);
            _context.Remove(cart);
            await this.SaveChanges(_context);
            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.DELETE),
                Status = (int)HttpStatusCode.OK,
            });
        }

        [HttpPost]
        [Route("UpdateCart")]
        [MasterAuth]
        public async Task<IActionResult> UpdateCart(int cartId, CartRequestDTO updateModel)
        {
            var cart = await _context.Carts
                .Include(x => x.CartDetails)
                .Where(x => x.Id == cartId)
                .FirstOrDefaultAsync();
            if (cart == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET),
                    Status = (int)HttpStatusCode.BadRequest,
                });
            }

            cart.ProductSizeId = updateModel.ProductSizeId;
            cart.Quantity = updateModel.Quantity;
            cart.UpdatedAt = DateTime.Now;

            _context.CartDetails.RemoveRange(cart.CartDetails);
            await this.SaveChanges(_context);
            List<CartDetail> newCartDetails = new List<CartDetail>();
            foreach (var subCart in updateModel.Toppings)
            {
                var getTopping = _context.Toppings
                                .Include(x => x.Products)
                                .Where(x => x.IsValid && x.Products.Where(x => x.Id == cart.ProductSize.ProductId).Count() != 0 && x.Id == subCart.Id)
                                .AsNoTracking()
                                .FirstOrDefault();
                if (getTopping == null)
                {
                    return BadRequest(new APIResponseBase
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Message = "Topping is not valid for this drink.",
                        IsSuccess = false
                    });
                }
                CartDetail detail = new CartDetail();
                detail.CartId = cart.Id;
                detail.ToppingId = getTopping.Id;
                detail.Quantity = subCart.Quantity;
                newCartDetails.Add(detail);
            }

            if (newCartDetails.Count > 0)
            {
                await _context.CartDetails.AddRangeAsync(newCartDetails);
            }

            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.PUT),
                Status = (int)HttpStatusCode.OK,
            });
        }
    }
}
