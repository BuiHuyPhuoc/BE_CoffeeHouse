using CoffeeHouseAPI.DTOs.AI;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Google.Api.Gax.ResourceNames;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        public AIController(DbcoffeeHouseContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("AIMenu")]
        public async Task<IActionResult> AIMenu()
        {
            var products = await _context.Products
                .Where(x => x.IsValid && x.Description2 != null && x.Material != null)
                .ToListAsync();

            List<AIProduct> aIProducts = products.Select(product => new AIProduct
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Description = Regex.Replace(product.Description2!.Trim(), @"\s+", " "),
                Material = Regex.Replace(product.Material!.Trim(), @"\s+", " ")
            }).ToList();

            return Ok(aIProducts);
        }
    }
}
