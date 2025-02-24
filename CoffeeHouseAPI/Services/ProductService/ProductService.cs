using AutoMapper;
using CoffeeHouseAPI.DTOs.AI;
using CoffeeHouseAPI.DTOs.Discount;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseLib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Services.ProductService
{
    public class ProductService : IProductService
    {
        readonly DbcoffeeHouseContext _context;

        public ProductService(DbcoffeeHouseContext context, IMapper mapper)
        {
            _context = context;
        }

        public List<Product> GetProductWithRelate(bool isNoTracking = false)
        {
            var query = _context.Products
                .Include(x => x.ImageDefaultNavigation)
                .Include(x => x.Images)
                .Include(x => x.Category)
                .Include(x => x.ProductSizes.Where(x => x.IsValid).OrderBy(y => y.Price))
                .Include(x => x.Toppings)
                .Include(x => x.Reviews)
                .Where(x => x.IsValid);

            if (isNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query.ToList();
        }
    }
}
