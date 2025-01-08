namespace CoffeeHouseAPI.DTOs.Auth
{
    public class CreateAccountRequest
    {
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = null!;
        public bool IdRole { get; set; } = false;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
