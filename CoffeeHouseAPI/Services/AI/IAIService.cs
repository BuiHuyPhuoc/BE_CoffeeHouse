using CoffeeHouseAPI.DTOs.AI;

namespace CoffeeHouseAPI.Services.AI
{
    public interface IAIService
    {
        public AIRecommendResponse GetRecommendProduct();
    }
}
