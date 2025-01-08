namespace CoffeeHouseAPI.DTOs.Category
{
    public class CategoryDTO
    {
        public int Id { get; set; }

        public string CategoryName { get; set; } = null!;

        public int? IdParent { get; set; }
    }
}
