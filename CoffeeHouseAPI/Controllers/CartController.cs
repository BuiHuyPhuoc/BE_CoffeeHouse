using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Cart;
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

        public CartController(DbcoffeeHouseContext context)
        {
            _context = context;
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

    }
}
