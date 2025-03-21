using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseAPI.Services.Firebase;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;
        readonly FirebaseService _firebaseService;

        public CategoryController(DbcoffeeHouseContext context, IMapper mapper, FirebaseService firebaseService)
        {
            _context = context;
            _mapper = mapper;
            _firebaseService = firebaseService;
        }

        [HttpGet]
        [Route("GetCategory")]
        public async Task<IActionResult> GetCategory()
        {
            var category = await _context.Categories.Include(x => x.InverseIdParentNavigation).Where(x => x.IdParent == null && x.IsValid).AsNoTracking().ToListAsync();
            List<CategoryResponseDTO> categoryDTO = _mapper.Map<List<CategoryResponseDTO>>(category);
            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Value = categoryDTO,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                IsSuccess = true
            });
        }

        [HttpGet]
        [Route("GetCateogryById")]
        public async Task<IActionResult> GetCateogryById([FromQuery] int id)
        {
            var category = await _context.Categories.Where(x => x.Id == id && x.IsValid).Include(x => x.InverseIdParentNavigation).AsNoTracking().FirstOrDefaultAsync();
            if (category == null)
            {
                return BadRequest(new APIResponseBase
                {
                    Status = (int)StatusCodes.Status400BadRequest,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.GET),
                    IsSuccess = false
                });
            }
            CategoryDTO categoryDTO = _mapper.Map<CategoryResponseDTO>(category);
            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Value = categoryDTO,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                IsSuccess = true
            });
        }

        [HttpPut]
        [Route("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory([FromQuery] int id, [FromBody] CategoryRequestDTO categoryDTO)
        {
            var category = await _context.Categories.Where(x => x.Id == id && x.IsValid).FirstOrDefaultAsync();

            if (category == null)
            {
                return BadRequest(new APIResponseBase
                {
                    Status = (int)StatusCodes.Status400BadRequest,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.PUT),
                    IsSuccess = false
                });
            }

            if (categoryDTO.IdParent == null && categoryDTO.Image == null)
            {
                return BadRequest(new APIResponseBase
                {
                    Status = (int)StatusCodes.Status400BadRequest,
                    Message = "Category lớp 1 (lớp cha) phải truyền ảnh",
                    IsSuccess = false
                });
            }

            if (categoryDTO.Image != null)
            {
                var imageUrl = await _firebaseService.UploadImageAsync(categoryDTO.Image);
                category.Image = imageUrl;
            }

            category.IdParent = categoryDTO.IdParent;
            category.CategoryName = categoryDTO.CategoryName;
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Value = _mapper.Map<CategoryResponseDTO>(category),
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.PUT),
                IsSuccess = true
            });
        }

        [HttpGet]
        [Route(nameof(GetParentCategory))]
        public IActionResult GetParentCategory()
        {
            var category = _context.Categories.Where(x => x.IdParent == null && x.IsValid).AsNoTracking().ToList();
            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Value = _mapper.Map<List<CategoryResponseDTO>>(category),
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                IsSuccess = true
            });
        }

        [HttpGet]
        [Route(nameof(GetChildCategory))]
        public IActionResult GetChildCategory()
        {
            var category = _context.Categories
                .Include(x => x.IdParentNavigation)
                .Where(x => x.IdParent != null && x.IsValid && x.IdParentNavigation != null && x.IdParentNavigation.IsValid).AsNoTracking().ToList();

            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Value = _mapper.Map<List<CategoryResponseDTO>>(category),
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                IsSuccess = true
            });
        }

        [HttpPost]
        [Route(nameof(CreateCategory))]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequestDTO categoryDTO)
        {
            if (categoryDTO.IdParent != null)
            {
                var parentCateogry = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == categoryDTO.IdParent);

                if (parentCateogry == null)
                {
                    return BadRequest(new APIResponseBase
                    {
                        Status = (int)StatusCodes.Status400BadRequest,
                        Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.POST),
                        IsSuccess = false
                    });
                }
            }
            else
            {
                if (categoryDTO.Image == null)
                {
                    return BadRequest(new APIResponseBase
                    {
                        Status = (int)StatusCodes.Status400BadRequest,
                        Message = "Category lớp 1 (lớp cha) phải truyền ảnh",
                        IsSuccess = false
                    });
                }
            }

            var category = _mapper.Map<Category>(categoryDTO);

            await _context.Categories.AddAsync(category);
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Value = _mapper.Map<CategoryResponseDTO>(category),
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.POST),
                IsSuccess = true
            });
        }

        [HttpDelete]
        [Route(nameof(DeleteCategory))]
        public async Task<IActionResult> DeleteCategory([FromQuery] int id)
        {
            var category = await _context.Categories.Where(x => x.Id == id && x.IsValid).FirstOrDefaultAsync();
            if (category == null)
            {
                return BadRequest(new APIResponseBase
                {
                    Status = (int)StatusCodes.Status400BadRequest,
                    Message = GENERATE_DATA.API_ACTION_RESPONSE(false, API_ACTION.DELETE),
                    IsSuccess = false
                });
            }
            category.IsValid = false;
            await this.SaveChanges(_context);

            return Ok(new APIResponseBase
            {
                Status = (int)StatusCodes.Status200OK,
                Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.DELETE),
                IsSuccess = true
            });
        }

    }
}
