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
            var categories = await _context.Categories.Include(x => x.Products).Where(x => x.IdParent != null && x.Products.Count != 0).ToListAsync();
            List<ProductByCategoryDTO> products = new List<ProductByCategoryDTO>();
            foreach (var category in categories)
            {
                ProductByCategoryDTO productResponseDTO = new ProductByCategoryDTO();
                productResponseDTO.Category = _mapper.Map<CategoryResponseDTO>(category);
                var productByCateogry = _context.Products.Include(x => x.Images.Take(1)).Where(x => x.CategoryId == category.Id).ToList();
                productResponseDTO.Products = _mapper.Map<List<ProductResponseDTO>>(productByCateogry);
                products.Add(productResponseDTO);
            }
            return Ok(products);
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

            if (request.Sizes.Count == 0)
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
            for (int i = 0; i< request.Images.Count; i++) {
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
            List<ProductSize> productSizes = _mapper.Map<List<ProductSize>>(request.Sizes);
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


        //[HttpGet]
        //[Route("GetProduct")]
        //public IActionResult GetProduct([FromQuery] int? categoryId)
        //{
        //    Category? category = _context.Categories.Where(x => x.Id == categoryId).FirstOrDefault() ?? null ;
        //    if (category == null)
        //    {
        //        var product = _context.Products.Include(x => x.Category).Where(x => x.IsValid).ToList();
        //        List<ProductDTO> productDTO = _mapper.Map<List<ProductDTO>>(product);
        //        if (product.Count == 0)
        //        {
        //            return NotFound(new APIResponseBase
        //            {
        //                IsSuccess = true,
        //                Status = (int)StatusCodes.Status404NotFound,
        //                Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET),
        //                Value = productDTO
        //            });
        //        }

        //        return Ok(new APIResponseBase
        //        {
        //            IsSuccess = true,
        //            Status = (int)StatusCodes.Status200OK,
        //            Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
        //            Value = productDTO
        //        });
        //    }
        //    else
        //    {
        //        var product = _context.Products.Include(x => x.Category).Where(x => x.IsValid && x.CategoryId == categoryId).ToList();
        //        List<ProductDTO> productDTO = _mapper.Map<List<ProductDTO>>(product);
        //        if (product.Count == 0)
        //        {
        //            return NotFound(new APIResponseBase
        //            {
        //                IsSuccess = true,
        //                Status = (int)StatusCodes.Status404NotFound,
        //                Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET),
        //                Value = productDTO
        //            });
        //        }

        //        return Ok(new APIResponseBase
        //        {
        //            IsSuccess = true,
        //            Status = (int)StatusCodes.Status200OK,
        //            Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
        //            Value = productDTO
        //        });
        //    }
        //}

    }
}
