using CoffeeHouseAPI.DTOs.Address;
using CoffeeHouseAPI.DTOs.Customer;
using CoffeeHouseAPI.DTOs.Voucher;
using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.DTOs.Order
{
    public class OrderResponseDTO
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; }

        public int? VoucherId { get; set; }

        public int AddressId { get; set; }

        public AddressDTO Address { get; set; } = null!;

        public CustomerDTO Customer { get; set; } = null!;

        public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();

        public List<OrderLogDTO> OrderLogs { get; set; } = new List<OrderLogDTO>();

        public VoucherDTO? Voucher { get; set; }
    }
}
