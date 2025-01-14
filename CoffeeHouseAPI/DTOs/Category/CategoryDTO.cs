namespace CoffeeHouseAPI.DTOs.Category
{
    public class CategoryDTO
    {
        public int? Id { get; set; }
        public string CategoryName { get; set; } = null!;
    }

    public class CategoryRequestDTO : CategoryDTO
    {
        public int? IdParent { get; set; }
    }

    public class CategoryResponseDTO : CategoryDTO
    {
    }
}
