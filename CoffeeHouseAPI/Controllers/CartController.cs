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
            LoginResponse loginResponse = this.GetLoginResponseFromHttpContext();

            var getCart = await _context.Carts
                                .Where(x => x.CustomerId == loginResponse.Id && x.ProductSizeId == request.ProductSizeId)
                                .FirstOrDefaultAsync();

            Cart newCart = new Cart();
            newCart.Quantity = request.Quantity;
            newCart.ProductSizeId = request.ProductSizeId;
            newCart.CustomerId = loginResponse.Id;
            newCart.CreatedAt = DateTime.Now;
            newCart.UpdatedAt = DateTime.Now;

            await this.SaveChanges(_context);

            List<CartDetail> newCartDetails = new List<CartDetail>();
            foreach (var subCart in request.Toppings)
            {
                CartDetail detail = new CartDetail();
                detail.CartId = newCart.Id;
                detail.ToppingId = subCart.Id;
                detail.Quantity = subCart.Quantity;
                newCartDetails.Add(detail);
            }

            if (newCartDetails.Count > 0)
            {
                await _context.CartDetails.AddRangeAsync(newCartDetails);
                await this.SaveChanges(_context);
            }

            return Ok(new APIResponseBase
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Add to cart success",
                IsSuccess = true
            });
        }

    }
}
