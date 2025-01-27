using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.Firebase;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

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
        public async Task<IActionResult> GetProduct(int? quantity)
        {
            var products = await _context.Products.Include(x => x.ProductSizes).Include(x => x.ImageDefaultNavigation)
                .Include(x => x.Category).Include(x => x.Images).ToListAsync();
            if (quantity != null)
            {
                products = products.Take((int)quantity).ToList();
            }
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

            var newProduct = _mapper.Map<Product>(request);
            newProduct.Images = new List<Image>();

            // Add image to firebase
            List<Image> images = _mapper.Map<List<Image>>(request.Images);
            for (int i = 0; i < request.Images.Count; i++)
            {
                string url = await _firebaseService.UploadImageAsync(request.Images[i]);
                images[i].FirebaseImage = url;
            }

            // Add default image to firebase
            Image imageDefault = _mapper.Map<Image>(request.ImageDefaultNavigation);
            string urlImageDefault = await _firebaseService.UploadImageAsync(request.ImageDefaultNavigation);
            imageDefault.FirebaseImage = urlImageDefault;

            // Add image to database
            _context.Images.AddRange(images);
            _context.Images.Add(imageDefault);
            await this.SaveChanges(_context);

            // Add product
            newProduct.Images = images;
            newProduct.ImageDefaultNavigation = imageDefault;
            _context.Products.Add(newProduct);
            await this.SaveChanges(_context);

            //// Add product size
            //List<ProductSize> productSizes = _mapper.Map<List<ProductSize>>(request.ProductSizes);
            //foreach (var productSize in productSizes)
            //{
            //    productSize.ProductId = newProduct.Id;
            //}
            //_context.ProductSizes.AddRange(productSizes);
            //await this.SaveChanges(_context);

            var productDTO = _mapper.Map<ProductResponseDTO>(newProduct);

            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)StatusCodes.Status200OK,
                Message = "Add product success",
                Value = productDTO
            });
        }

        [HttpGet]
        [Route("GetProductDetail")]
        public IActionResult GetProductDetail(int idProduct)
        {
            var product = _context.Products
                .Include(x => x.Toppings)
                .Include(x => x.ImageDefaultNavigation)
                .Include(x => x.ProductDiscounts)
                .Include(x => x.ProductSizes.OrderBy(y => y.Price))
                .Include(x => x.Category)
                .Include(x => x.Toppings)
                .Where(x => x.Id == idProduct).AsNoTracking().FirstOrDefault();
            if (product == null)
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = true,
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET)
                });
            }
            var productDTO = _mapper.Map<ProductResponseDTO>(product);
            return Ok(new APIResponseBase
            {
                IsSuccess = true,
                Status = (int)HttpStatusCode.OK,
                Value = productDTO,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET)
            });
        }
    }
}
