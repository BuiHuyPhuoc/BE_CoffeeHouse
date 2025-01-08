using AutoMapper;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.DTOs.Category;
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

        public CategoryController(DbcoffeeHouseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetCateogry")]
        public async Task<IActionResult> GetCateogry()
        {
            var category = await _context.Categories.ToListAsync();
            List<CategoryDTO> categoryDTO = _mapper.Map<List<CategoryDTO>>(category);
            return Ok(new APIReponse {
                Status = (int)StatusCodes.Status200OK,
                Value = categoryDTO,
                Message = "Get cateogry success",
                IsSuccess = true
            });
        }

        [HttpGet]
        [Route("GetCateogryById")]
        public async Task<IActionResult> GetCateogryById([FromQuery] int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return BadRequest(new APIReponse
                {
                    Status = (int)StatusCodes.Status400BadRequest,
                    Message = "Get cateogry failed",
                    IsSuccess = false
                });
            }
            CategoryDTO categoryDTO = _mapper.Map<CategoryDTO>(category);
            return Ok(new APIReponse
            {
                Status = (int)StatusCodes.Status200OK,
                Value = categoryDTO,
                Message = "Get cateogry success",
                IsSuccess = true
            });
        }
    }
}
