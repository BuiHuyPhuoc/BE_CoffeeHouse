using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.ProductService
{
    public interface IProductService
    {
        List<Product> GetProductWithRelate();
    }
}
