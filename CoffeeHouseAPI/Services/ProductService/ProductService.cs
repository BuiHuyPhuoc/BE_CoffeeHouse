using AutoMapper;
using CoffeeHouseAPI.DTOs.AI;
using CoffeeHouseLib.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Services.ProductService
{
    public class ProductService : IProductService
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _mapper;

        public ProductService(DbcoffeeHouseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<Product> GetProductWithRelate()
        {
            return _context.Products
                .Include(x => x.Toppings.Where(x => x.IsValid))
                .Include(x => x.ImageDefaultNavigation)
                .Include(x => x.ProductDiscounts.Where(x => x.IsActive))
                .Include(x => x.ProductSizes.OrderBy(y => y.Price).Where(y => y.IsValid))
                .Include(x => x.Category)
                .AsNoTracking()
                .ToList();
        }
    }
}
