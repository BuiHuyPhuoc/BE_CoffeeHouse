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
                .Where(x => x.IsValid)
                .Select(x => new Product
                {
                    Id = x.Id,
                    ProductName = x.ProductName,
                    Discount = x.Discount != null && x.Discount.IsActive ? x.Discount : null,
                    ImageDefaultNavigation = x.ImageDefaultNavigation,
                    Images = x.Images,
                    Category = x.Category,
                    ProductSizes = x.ProductSizes.Where(ps => ps.IsValid).OrderBy(ps => ps.Price).ToList(),
                    Toppings = x.Toppings,
                    CategoryId = x.Category.Id,
                    CreatedAt = x.CreatedAt,
                    Description = x.Description,
                    Description2 = x.Description2,
                    IsValid = x.IsValid,
                    Material = x.Material
                });

            if (isNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query.ToList();
        }
    }
}
