namespace CoffeeHouseAPI.DTOs.Image
{
    public class ImageDTO
    {
        public string? ImageName { get; set; } = null!;
        public string ImageType { get; set; } = null!;
        public int ImageClassId { get; set; }
    }
    public class ImageRequestDTO : ImageDTO
    {
        public string Content { get; set; } = null!;
    }

    public class ImageResponseDTO : ImageDTO
    {
        public string FirebaseImage { get; set; } = null!;
    }
}
