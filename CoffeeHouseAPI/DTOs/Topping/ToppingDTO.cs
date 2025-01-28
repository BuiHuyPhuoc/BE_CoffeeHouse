namespace CoffeeHouseAPI.DTOs.Topping
{
    public class ToppingDTO
    {
        public int? Id { get; set; }

        public string ToppingName { get; set; } = null!;

        public decimal ToppingPrice { get; set; }

        public bool IsValid { get; set; } = true;
    }

    public class ToppingOrderDTO : ToppingDTO
    {
        public int Quantity { get; set; } = 1;
    }
}
