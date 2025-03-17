using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.ProductService
{
    public interface IProductService
    {
        List<Product> GetProductWithRelate(bool isNoTracking = false);

        List<Product> GetTopSaleProductWithRelate(bool isNoTracking = false);

        decimal GetProductPrice(Product product, ProductSize productSize);
    }
}
