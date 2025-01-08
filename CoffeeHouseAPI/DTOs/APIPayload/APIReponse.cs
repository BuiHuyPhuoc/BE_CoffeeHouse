namespace CoffeeHouseAPI.DTOs.APIPayload
{
    public class APIReponse
    {
        public APIReponse() { }
        public int Status { get; set; }
        public string Message { get; set; } = null!;
        public bool IsSuccess { get; set; }
        public object Value { get; set; } = null!;
    }
}
