using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Review;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.ProductService;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IProductService _productService;
        readonly IMapper _mapper;

        public ReviewController(DbcoffeeHouseContext context, IProductService productService, IMapper mapper)
        {
            _context = context;
            _productService = productService;
            _mapper = mapper;
        }

        /// <summary>
        /// Last test: 24/2/2025
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(AddReview))]
        [MasterAuth]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO request)
        {
            var product = _productService.GetProductWithRelate().Where(x => x.Id == request.ProductId).FirstOrDefault();

            if (product == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Status = (int)StatusCodes.Status404NotFound,
                    Message = "Sản phẩm không tồn tại"
                });
            }

            if (request.Rating < 0 || request.Rating > 5)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Status = (int)StatusCodes.Status404NotFound,
                    Message = "Đánh giá từ 1 đến 5 sao."
                });
            }

            var loginResponse = this.GetLoginResponseFromHttpContext();

            var newReview = _mapper.Map<Review>(request);
            newReview.UpdatedAt = DateTime.Now;
            newReview.CreatedAt = DateTime.Now;
            newReview.CustomerId = loginResponse.Id;

            _context.Reviews.Add(newReview);
            await this.SaveChanges(_context);

            var newReviewDTO = _mapper.Map<ReviewDTO>(newReview);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)StatusCodes.Status200OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
            });
        }

        [HttpGet]
        [Route(nameof(GetReview))]
        public IActionResult GetReview(int id, int quantity = 5)
        {
            var product = _productService.GetProductWithRelate().Where(x => x.Id == id).FirstOrDefault();

            if (product == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Sản phẩm không tồn tại",
                    Status = (int)StatusCodes.Status400BadRequest
                });
            }

            var review = _context.Reviews.Where(x => x.ProductId == id).Take(quantity).ToList();
            var reviewDTO = _mapper.Map<List<ReviewDTO>>(review);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Status = (int)StatusCodes.Status200OK,
                Value = reviewDTO
            });
        }
    }
}
