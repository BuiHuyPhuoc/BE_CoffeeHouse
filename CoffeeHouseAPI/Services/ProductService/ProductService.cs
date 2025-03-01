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
                .Include(x => x.Discount)
                .Where(x => x.IsValid);

            if (isNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query.ToList();
        }

        public List<Product> GetTopSaleProductWithRelate(bool isNoTracking = false)
        {
            var orderdetail = _context.OrderDetails
                .Include(x => x.Order)
                .Where(x => x.Order.OrderDate <= DateTime.UtcNow && x.Order.OrderDate >= DateTime.UtcNow.AddMonths(-3)).AsNoTracking();

            // Bước 1: Lấy danh sách `ProductId` phổ biến nhất
            var topProductIds = _context.OrderDetails
                .Join(_context.ProductSizes,
                    orderDetail => orderDetail.ProductSizeId,
                    productSize => productSize.Id,
                    (orderDetail, productSize) => productSize.ProductId)
                .GroupBy(productId => productId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .AsNoTracking()
                .Select(g => g.Key)
                .ToList();

            // Bước 2: Lấy tất cả thông tin sản phẩm với `Include`
            var topProducts = _context.Products
                .Where(p => topProductIds.Contains(p.Id))
                .Include(x => x.ImageDefaultNavigation)
                .Include(x => x.Images)
                .Include(x => x.Category)
                .Include(x => x.ProductSizes.Where(x => x.IsValid).OrderBy(y => y.Price))
                .Include(x => x.Toppings)
                .Include(x => x.Reviews)
                .Include(x => x.Discount)
                .AsNoTracking()
                .ToList();

            return topProducts;
        }
    }
}
