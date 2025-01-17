using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.Firebase;
using CoffeeHouseLib.Models;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : TCHControllerBase
    {

        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;
        readonly FirebaseService _firebaseService;

        public ProductController(DbcoffeeHouseContext context, IMapper mapper, FirebaseService firebaseService)
        {
            _context = context;
            _mapper = mapper;
            _firebaseService = firebaseService;
        }

        [HttpGet]
        [Route("GetProduct")]
        public async Task<IActionResult> GetProduct()
        {
            var products = await _context.Products.Include(x => x.ProductSizes.Where(x => x.IsValid)).Include(x => x.ImageDefaultNavigation)
                .Include(x => x.Category).Include(x => x.Images).Where(x => x.IsValid).ToListAsync();
            var productDTOs = _mapper.Map<List<ProductResponseDTO>>(products);
            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = productDTOs,
                IsSuccess = true
            });

        }

        [HttpPost]
        [Route("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductRequestDTO request)
        {
            if (request.Images.Count == 0)
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Insert at least one images",
                    Status = (int)StatusCodes.Status400BadRequest
                });

            if (request.ProductSizes.Count == 0)
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Insert at least one sizes",
                    Status = (int)StatusCodes.Status400BadRequest
                });

            ProductDTO productDTO = new ProductDTO();
            productDTO.ProductName = request.ProductName;
            productDTO.Description = request.Description;
            productDTO.CategoryId = request.CategoryId;
            productDTO.IsValid = true;

            var newProduct = _mapper.Map<Product>(productDTO);

            // Add image to firebase
            List<Image> images = _mapper.Map<List<Image>>(request.Images);
            for (int i = 0; i < request.Images.Count; i++)
            {
                string url = await _firebaseService.UploadImageAsync(request.Images[i]);
                images[i].FirebaseImage = url;
            }

            // Add image to database
            _context.Images.AddRange(images);
            await this.SaveChanges(_context);

            // Add product
            newProduct.Images = images;
            _context.Products.Add(newProduct);
            await this.SaveChanges(_context);

            // Add product size
            List<ProductSize> productSizes = _mapper.Map<List<ProductSize>>(request.ProductSizes);
            foreach (var productSize in productSizes)
            {
                productSize.ProductId = newProduct.Id;
            }
            _context.ProductSizes.AddRange(productSizes);
            await this.SaveChanges(_context);

            productDTO = _mapper.Map<ProductDTO>(newProduct);
            productDTO.Images = new List<ImageRequestDTO>();

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)StatusCodes.Status200OK,
                Message = "Add product success",
                Value = productDTO
            });
        }

        [HttpGet]
        [Route("GetDetailProduct")]
        public async Task<IActionResult> GetDetailProduct(int idProduct)
        {
            var product = await _context.Products.Include(x => x.ProductSizes.Where(x => x.IsValid)).Include(x => x.ImageDefaultNavigation)
                .Include(x => x.Category).Include(x => x.Images).Where(x => x.IsValid && x.Id == idProduct).FirstOrDefaultAsync();
            var productDTO = _mapper.Map<ProductResponseDTO>(product);
            if (product == null)
            {
                return NotFound(new APIResponseBase
                {
                    Status = (int)StatusCodes.Status404NotFound,
                    IsSuccess = false,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET)
                });
            }

            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                Value = productDTO,
                IsSuccess = true
            });
        }
    }
}
